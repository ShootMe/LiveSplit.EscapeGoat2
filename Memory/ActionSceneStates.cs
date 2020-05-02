namespace LiveSplit.EscapeGoat2 {
    public enum ActionSceneStates {
        Uninitialized,
        ShowingIntroText,
        CheckForRegionMovement,
        FadingCurrentRegionOut,
        ExitingCurrentRoom,
        CheckForRoomMovement,
        RegionLoadedDuringRegionTransition,
        RoomLoadedWaitingOnBlack,
        FadingRoomIn,
        Action,
        ReturningToHub,
        PlayerDead,
        QuitGameRequested,
        Quitting,
        BeatenGame
    }
}