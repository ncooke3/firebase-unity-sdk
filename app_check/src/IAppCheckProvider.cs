/*
 * Copyright 2023 Google LLC
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Firebase.AppCheck {

/// @brief Interface for a provider that generates {@link AppCheckToken}s.
///
/// This provider can be called at any time by any Firebase library that depends
/// (optionally or  otherwise) on {@link AppCheckToken}s. This provider is
/// responsible for determining if it can create a new token at the time of the
/// call and returning that new token if it can.
public interface IAppCheckProvider {
  /// Returns an AppCheckToken or throws an exception with an error code and
  /// error message.
  System.Threading.Tasks.Task<AppCheckToken> GetTokenAsync();
}

}
