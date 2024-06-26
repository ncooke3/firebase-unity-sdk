// Copyright 2021 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Firebase.Sample.Firestore {
  class EventAccumulator<T> {
    private TaskCompletionSource<object> completion;
    private readonly List<T> events = new List<T>();
    private int maxEvents = 0;
    private bool assertOnAnyEvent = false;
    private int mainThreadId = -1;
    private Action<string> onAssertionFailure;

    public EventAccumulator(int mainThreadId, Action<string> onAssertionFailure) {
      this.mainThreadId = mainThreadId;
      this.onAssertionFailure = onAssertionFailure;
    }

    /// <summary>
    /// Returns a listener callback suitable for passing to Listen().
    /// </summary>
    public Action<T> Listener {
      get {
        return (value) => {
          lock (this) {
            HardAssert(!assertOnAnyEvent, "Received unexpected event: " + value);
            if (mainThreadId >= 0) {
              HardAssert(mainThreadId == Thread.CurrentThread.ManagedThreadId, "Listener callback from non-main thread.");
            }
            events.Add(value);
            CheckFulfilled();
          }
        };
      }
    }

    /// <summary>
    /// Waits for the specified number of events and returns them.
    /// </summary>
    public List<T> Await(int numEvents) {
      lock (this) {
        HardAssert(completion == null, "calling await while another await is running");
        completion = new TaskCompletionSource<object>();
        maxEvents = maxEvents + numEvents;
        CheckFulfilled();
      }

      completion.Task.Wait();

      lock (this) {
        completion = null;
        return events.GetRange(maxEvents - numEvents, numEvents);
      }
    }

    /// <summary>
    /// Waits for an event and returns it.
    /// </summary>
    public T Await() {
      return Await(1)[0];
    }

    /// <summary>
    /// Throw an exception if any more events are received.
    /// </summary>
    public void ThrowOnAnyEvent() {
      lock (this) {
        HardAssert(events.Count == maxEvents, "Received unexpected event: " + events.Last());
        assertOnAnyEvent = true;
      }
    }

    private void CheckFulfilled() {
      lock (this) {
        if (completion != null && events.Count >= maxEvents) {
          completion.SetResult(null);
        }
      }
    }

    private void HardAssert(bool condition, string message) {
      if (!condition) {
        string text = "Assertion Failure: " + message;
        UnityEngine.Debug.Log(text);
        onAssertionFailure(text);
      }
    }
  }
}
