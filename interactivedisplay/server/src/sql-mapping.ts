const COL_RED = '#E53935';
const COL_GREEN = '#43A047';
const COL_BLUE = '#1E88E5';
const COL_YELLOW = '#FDD835';
const COL_PURPLE = '#8E24AA';
const COL_ORANGE = '#F4511E';
const COL_TEAL = '#00897B';
const COL_WHITE = '#FFFFFF';
const COL_BLACK = '#000000';


export enum DataRepresentation {
    Metric, Categorical
}

interface Gradient {
    stop: number;
    color: string;
}

interface Bin {
    displayName: string;
    value?: number;
    range?: [number, number];
}

interface ValueMapping {
    dbValue: string;
    name: string;
    color: string;
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
    bins: Bin[];
    gradient: Gradient[];
}

export type SqlColumnMapping = CategoricalSqlMapping | MetricSqlMapping;

let portionBins: Bin[] = [
    {
        displayName: '0',
        value: 0
    },
    {
        displayName: '1',
        value: 1
    },
    {
        displayName: '2',
        value: 2
    },
    {
        displayName: '3',
        value: 3
    },
    {
        displayName: '4',
        value: 4
    },
    {
        displayName: '5+',
        range: [5, 1000]
    },
];

let portionGradient: Gradient[] = [
    {
        stop: 0,
        color: COL_WHITE
    },
    {
        stop: 20,
        color: COL_BLUE
    }
];

let ratingBins: Bin[] = [
    {
        displayName: 'Very good',
        value: 4
    },
    {
        displayName: 'Good',
        value: 3
    },
    {
        displayName: 'Neutral',
        value: 2
    },
    {
        displayName: 'Bad',
        value: 1
    },
    {
        displayName: 'Very bad',
        value: 0
    }
];

let ratingGradient: Gradient[] = [
    {
        stop: 0,
        color: COL_RED
    },
    {
        stop: 4,
        color: COL_GREEN
    }
];



export const SmartactMapping: SqlColumnMapping[] = [
    {
        dbColumn: 'Phase',
        name: 'Phase',
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: '1',
                name: 'Baseline 1',
                color: COL_RED
            },
            {
                dbValue: '2',
                name: 'Intervention',
                color: COL_BLUE
            },
            {
                dbValue: '3',
                name: 'Baseline 2',
                color: COL_GREEN
            },
            {
                dbValue: '4',
                name: 'Pause',
                color: COL_YELLOW
            },
            {
                dbValue: '5',
                name: 'Baseline 3',
                color: COL_PURPLE
            }
        ]
    },
    {
        dbColumn: 'Cond',
        name: 'Kondition',
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: '1',
                name: 'Mit Happy Eater',
                color: COL_BLUE
            },
            {
                dbValue: '2',
                name: 'Ohne Happy Eater',
                color: COL_RED
            }
        ]
    },
    {
        dbColumn: 'MahlzNr',
        name: 'Mahlzeit',
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: '1',
                name: 'Frühstück',
                color: COL_BLUE
            },
            {
                dbValue: '2',
                name: 'Mittagessen',
                color: COL_YELLOW
            },
            {
                dbValue: '3',
                name: 'Abendessen',
                color: COL_RED,
            },
            {
                dbValue: '4',
                name: 'Kaffe & Kuchen',
                color: COL_PURPLE
            },
            {
                dbValue: '5',
                name: 'Snacks',
                color: COL_GREEN
            }
        ]
    },
    {
        dbColumn: 'Time',
        name: 'Uhrzeit',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 2400000000, // 24:00:00.0000
        bins: [
            {
                displayName: 'Morning',
                range: [0, 600000000]
            },
            {
                displayName: 'Noon',
                range: [600000001, 1200000000]
            },
            {
                displayName: 'Evening',
                range: [1200000001, 1800000000]
            },
            {
                displayName: 'Night',
                range: [1800000001, 2400000000]
            },
        ],
        gradient: [
            {
                stop: 0,
                color: COL_BLACK
            },
            {
                stop: 600000000, // 06:00:00.0000
                color: COL_ORANGE
            },
            {
                stop: 1200000000, // 12:00:00.0000
                color: COL_BLUE
            },
            {
                stop: 1800000000, // 18:00:00.0000
                color: COL_PURPLE
            },
            {
                stop: 2400000000, // 24:00:00.0000
                color: COL_BLACK
            }
        ]
    },

    // TODO: Richtige Spalte??
    {
        dbColumn: 'Dur_HE',
        name: 'Dauer der Mahlzeit',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 86072,
        bins: [
            {
                displayName: "< 1 Minutes",
                range: [0, 60]
            },
            {
                displayName: "< 5 Minutes",
                range: [61, 60 * 5]
            },
            {
                displayName: "< 10 Minutes",
                range: [60 * 5 + 1, 60 * 10]
            },
            {
                displayName: "> 10 Minutes",
                range: [60 * 10 + 1, 60 * 60 * 24]
            }
        ],
        gradient: [
            {
                stop: 0,
                color: COL_WHITE
            },
            {
                stop: 86072,
                color: COL_BLUE
            }
        ]
    },


 
    // TODO: Namen + Min,Max
    {
        dbColumn: 'Sum_Getr_1',
        name: 'Portionen Getreide',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 20,

        bins: portionBins,
        gradient: portionGradient
    },
    {
        dbColumn: 'Sum_Obst_1',
        name: 'Portionen Obst',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 20,

        bins: portionBins,
        gradient: portionGradient
    },
    {
        dbColumn: 'Sum_Gemu_1',
        name: 'Portionen Gemüse',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 20,

        bins: portionBins,
        gradient: portionGradient
    },
    {
        dbColumn: 'Sum_Flei_1',
        name: 'Portionen Fleisch',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 20,

        bins: portionBins,
        gradient: portionGradient
    },
    {
        dbColumn: 'Sum_Milch_1',
        name: 'Portionen Milch',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 20,

        bins: portionBins,
        gradient: portionGradient
    },
    {
        dbColumn: 'Sum_Extra_1',
        name: 'Portionen Extra',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 20,

        bins: portionBins,
        gradient: portionGradient
    },

    // TODO: not available?
    // {
    //     dbColumn: 'Cal',
    //     name: 'Kalorien',
    //     type: DataRepresentation.Metric,
    //     minValue: 0,
    //     maxValue: 66,

    //     bins: [],
    //     gradient: []
    // },

    {
        dbColumn: 'WT',
        name: 'Wochentag',
        type: DataRepresentation.Categorical,
        values: [
            {
                dbValue: '7',
                name: 'Montag',
                color: COL_RED
            },
            {
                dbValue: '6',
                name: 'Dienstag',
                color: COL_GREEN
            },
            {
                dbValue: '5',
                name: 'Mittwoch',
                color: COL_BLUE
            },
            {
                dbValue: '4',
                name: 'Donnerstag',
                color: COL_YELLOW
            },
            {
                dbValue: '3',
                name: 'Freitag',
                color: COL_PURPLE
            },
            {
                dbValue: '2',
                name: 'Samstag',
                color: COL_ORANGE
            },
            {
                dbValue: '1',
                name: 'Sonntag',
                color: COL_TEAL
            }
        ]
    },




    // TODO: names?
    {
        dbColumn: 'TEMS_App',
        name: 'Bewertung App?',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Gewo',
        name: 'Bewertung Gewohnheit',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Hu',
        name: 'Bewertung Hunger',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Gesu',
        name: 'Bewertung Gesund',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Aufw',
        name: 'Bewertung Aufwand',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Freud',
        name: 'Bewertung Freud?',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Trad',
        name: 'Bewertung Trad',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Eth',
        name: 'Bewertung Eth',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Gesell',
        name: 'Bewertung Gesellschaft',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Preis',
        name: 'Bewertung Preis',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Angespr',
        name: 'Bewertung Angespr?',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Gk',
        name: 'Bewertung Gk?',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Emot',
        name: 'Bewertung Emot?',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Erw',
        name: 'Bewertung Erw?',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    },
    {
        dbColumn: 'TEMS_Andg',
        name: 'Bewertung Andg?',
        type: DataRepresentation.Metric,
        minValue: 0,
        maxValue: 4,

        bins: ratingBins,
        gradient: ratingGradient
    }
];
