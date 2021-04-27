namespace HomieWrapper.Domekt200 {
    enum KomfoventRegisters : ushort {
        // These are all -1'ed, because that's how Modbus works...

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
