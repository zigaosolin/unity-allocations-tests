# unity-allocations-tests
Simple tests to show how much allocations are done for specific operations in C# in Unity environment.

Should work in any version of Unity.

Check Scripts/Editor/AllocationsTests.cs to see what the tests actually do. I will be happy if you expand some tests.

To run tests, go to Window -> General -> Test Runner -> Run All


Results on my computer (they are not always the same but no-allocations results mean no allocations):

- Allocated 0,82 MB: Closure with single capture creation for at 60 FPS for 120 s of game

- Allocated 0,38 MB: Coroutine start per frame at 60 FPS for 120 s of game

- Allocated 0,38 MB: Creating a 24 byte controller each frame at 60 FPS for 120 s of game

- Allocated 2,96 MB: Event add/remove for each frame at 60 FPS for 120 s of game

- No allocations: Foreach lambda allocation for at 60 FPS for 120 s of game

- No allocations: For each per frame on array at 60 FPS for 120 s of game

- No allocations: For each per frame on list at 60 FPS for 120 s of game

- No allocations: Event add/remove for each frame at 60 FPS for 120 s of game

- Allocated 0,21 MB: One Int to string conversion per frame at 60 FPS for 120 s of game

- No allocations: Lambda creation for at 60 FPS for 120 s of game

- Allocated 0,51 MB: Each frame select + sum on list of length 100 at 60 FPS for 120 s of game

- No allocations: Local closure with single capture creation for at 60 FPS for 120 s of game

- No allocations: Local function creation for each frame at 60 FPS for 120 s of game

- No allocations: Reusing a 24 byte controller each frame at 60 FPS for 120 s of game

- Allocated 0,83 MB: Builder combining 10 integers per frame at 60 FPS for 120 s of game

- Allocated 0,31 MB: Builder combining 10 integers with capacity 400 per frame at 60 FPS for 120 s of game

- Allocated 2,54 MB: Builder reuse clear combining 10 integers per frame at 60 FPS for 120 s of game

- Allocated 1,89 MB: String combining 10 integers per frame at 60 FPS for 120 s of game
