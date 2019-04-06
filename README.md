# unity-allocations-tests
Simple tests to show how much allocations are done for specific operations in C# in Unity environment.

Should work in any version of Unity.

Check Scripts/Editor/AllocationsTests

To run tests, go to Window -> General -> Test Runner -> Run All


Results on my computer (they are not always the same but no-allocations results mean no allocations):

Allocated 0,82 MB: Closure with single capture creation for at 60 FPS for 120 s of game

Allocated 0,37 MB: Coroutine start per frame at 60 FPS for 120 s of game

Allocated 0,37 MB: Creating a 24 byte controller each frame at 60 FPS for 120 s of game

No allocations: Foreach lambda allocation for at 60 FPS for 120 s of game

No allocations: For each per frame on array at 60 FPS for 120 s of game

No allocations: For each per frame on list at 60 FPS for 120 s of game

Allocated 0,25 MB: One Int to string conversion per frame at 60 FPS for 120 s of game

No allocations: Lambda creation for at 60 FPS for 120 s of game

Allocated 0,51 MB: Each frame select + sum on list of length 100 at 60 FPS for 120 s of game

No allocations: Local closure with single capture creation for at 60 FPS for 120 s of game

No allocations: Local function creation for each frame at 60 FPS for 120 s of game

No allocations: Reusing a 24 byte controller each frame at 60 FPS for 120 s of game

Allocated 0,81 MB: Builder combining 10 integers per frame at 60 FPS for 120 s of game

Allocated 0,28 MB: Builder combining 10 integers with capacity 400 per frame at 60 FPS for 120 s of game

Allocated 2,55 MB: Builder reuse clear combining 10 integers per frame at 60 FPS for 120 s of game

Allocated 1,93 MB: String combining 10 integers per frame at 60 FPS for 120 s of game