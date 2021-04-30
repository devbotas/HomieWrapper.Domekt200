namespace HomieWrapper {
    enum KomfoventRegisters : ushort {
        // General section.
        StartStop = 10000,
        Season,
        AlarmStatusWarnings,
        C4Configuration,
        HourAndMinute,
        DayOfWeek,
        MonthAndDay,
        Year,
        AlarmStatusStopFlags,
        AlarmStatusStopCode,
        ModbusAddress,
        ExternalHeaterControlSignalType,

        // Ventilation section.
        VentilationLevelManual = 10100,
        VentilationLevelCurrent,
        ModeAutoManual,

        // Temperatures section.
        SupplyAirTemperature = 10300
    }
}
