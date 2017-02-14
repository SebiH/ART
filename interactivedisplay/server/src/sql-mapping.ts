export enum DataRepresentation {
    Metric, Categorical
}

interface ValueMapping {
    dbValue: string;
    name: string;
}

interface CategoricalSqlMapping {
    type: DataRepresentation.Categorical;
    dbColumn: string;
    name: string;

    values: ValueMapping[];
}


interface MetricSqlMapping {
    type: DataRepresentation.Metric;
    dbColumn: string;
    name: string;

    minValue: number;
    maxValue: number;
}

export type SqlColumnMapping = CategoricalSqlMapping | MetricSqlMapping;


export const SmartactMapping: SqlColumnMapping[] = [
    {
        dbColumn: "Phase",
        name: "Phase",
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: "1",
                name: "Baseline 1"
            },
            {
                dbValue: "2",
                name: "Intervention"
            },
            {
                dbValue: "3",
                name: "Baseline 2"
            },
            {
                dbValue: "4",
                name: "Pause"
            },
            {
                dbValue: "5",
                name: "Baseline 3"
            },
            {
                // TODO ?
                dbValue: "6",
                name: "Unknown"
            }
        ]
    },
    {
        dbColumn: "Cond",
        name: "Kondition",
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: "1",
                name: "Mit Happy Eater"
            },
            {
                dbValue: "2",
                name: "Ohne Happy Eater"
            }
        ]
    },
    {
        dbColumn: "MahlzNr",
        name: "Mahlzeit",
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: "1",
                name: "Frühstück"
            },
            {
                dbValue: "2",
                name: "Mittagessen"
            },
            {
                dbValue: "3",
                name: "Abendessen"
            },
            {
                dbValue: "4",
                name: "Kaffe & Kuchen"
            },
            {
                dbValue: "5",
                name: "Snacks"
            },

            // TODO: ?
            {
                dbValue: "6",
                name: "Unbekannt"
            }
        ]
    },
    {
        dbColumn: "Time",
        name: "Uhrzeit",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 24000
    },

    // TODO: Richtige Spalte??
    {
        dbColumn: "Dur_HE",
        name: "Dauer der Mahlzeit",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 24000
    },


 
    // TODO: Namen + Min,Max
    {
        dbColumn: "Sum_Getr_1",
        name: "Portionen Getreide",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "Sum_Obst_1",
        name: "Portionen Obst",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "Sum_Gemu_1",
        name: "Portionen Gemüse",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "Sum_Flei_1",
        name: "Portionen Fleisch",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "Sum_Milch_1",
        name: "Portionen Milch",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "Sum_Extra_1",
        name: "Portionen Extra",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },

    // TODO: not available?
    // {
    //     dbColumn: "Calories",
    //     name: "Kalorien",
    //     type: DataRepresentation.Metric,
    //     minValue: 0,
    //     maxValue: 5000
    // },

    {
        dbColumn: "WT",
        name: "Wochentag",
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: "7",
                name: "Montag"
            },
            {
                dbValue: "6",
                name: "Dienstag"
            },
            {
                dbValue: "5",
                name: "Mittwoch"
            },
            {
                dbValue: "4",
                name: "Donnerstag"
            },
            {
                dbValue: "3",
                name: "Freitag"
            },
            {
                dbValue: "2",
                name: "Samstag"
            },
            {
                dbValue: "1",
                name: "Sonntag"
            }
        ]
    },




    // TODO: names?
    {
        dbColumn: "TEMS_App",
        name: "Bewertung App?",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Gewo",
        name: "Bewertung Gewohnheit",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Hu",
        name: "Bewertung Hunger",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Gesu",
        name: "Bewertung Gesund",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Aufw",
        name: "Bewertung Aufwand",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Freud",
        name: "Bewertung Freud?",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Trad",
        name: "Bewertung Trad",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Eth",
        name: "Bewertung Eth",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Gesell",
        name: "Bewertung Gesellschaft",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Preis",
        name: "Bewertung Preis",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Angespr",
        name: "Bewertung Angespr?",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Gk",
        name: "Bewertung Gk?",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Emot",
        name: "Bewertung Emot?",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Erw",
        name: "Bewertung Erw?",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    },
    {
        dbColumn: "TEMS_Andg",
        name: "Bewertung Andg?",
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 10
    }
];
