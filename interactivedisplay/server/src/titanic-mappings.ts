import { Bin, CategoricalSqlMapping, DataRepresentation, Gradient, MetricSqlMapping, SqlColumnMapping, ValueMapping } from './sql-mapping';
import * as Colors from './colors';

let numberConverter = (d: any) => +d;

export const TitanicMapping: SqlColumnMapping[] = [
    {
        dbColumn: 'Survived',
        name: 'Survival Status',
        type: DataRepresentation.Categorical,
        converter: numberConverter,
        values: [
            {
                dbValue: 0,
                name: 'Killed',
                color: Colors.Red
            },
            {
                dbValue: 1,
                name: 'Survived',
                color: Colors.Green
            }
        ]
    },
    {
        dbColumn: 'Pclass',
        name: 'Ticket Class',
        type: DataRepresentation.Categorical,
        converter: numberConverter,
        values: [
            {
                dbValue: 1,
                name: '1st class',
                color: Colors.Red
            },
            {
                dbValue: 2,
                name: '2nd class',
                color: Colors.Green,
            },
            {
                dbValue: 3,
                name: '3rd class',
                color: Colors.Blue
            },
        ]
    },
    {
        dbColumn: 'Sex',
        name: 'Sex',
        type: DataRepresentation.Categorical,
        converter: (d: any) => {
            if (d == 'female') { return 0; }
            else  if (d == 'male') { return 1; }
            else { console.error('Unexpected value for column "Sex": ' + d); return 0; }
        },
        values: [
            {
                dbValue: 0,
                name: 'Female',
                color: Colors.Pink
            },
            {
                dbValue: 1,
                name: 'Male',
                color: Colors.Blue
            }
        ]
    },
    {
        dbColumn: 'Age',
        name: 'Age',
        type: DataRepresentation.Metric,
        isTimeBased: false,
        converter: numberConverter,
        minValue: 0,
        maxValue: 80,
        ticks: [ 0, 10, 20, 30, 40, 50, 60, 70, 80 ],
        bins: [
            { displayName: 'Unknown', value: 0 },
            { displayName: '0-10', range: [0.1, 10] },
            { displayName: '10-20', range: [10, 20] },
            { displayName: '20-30', range: [20, 30] },
            { displayName: '30-40', range: [30, 40] },
            { displayName: '40-50', range: [40, 50] },
            { displayName: '50-60', range: [50, 60] },
            { displayName: '60-70', range: [60, 70] },
            { displayName: '70-80', range: [70, 80] }
        ],
        gradient: [
            {
                stop: 0,
                color: Colors.Green
            },
            {
                stop: 0.5,
                color: Colors.Yellow
            },
            {
                stop: 1,
                color: Colors.Red
            }
        ]
    },
    {
        dbColumn: 'SibSp',
        name: '# of Siblings / Spouses aboard',
        type: DataRepresentation.Metric,
        isTimeBased: false,
        converter: numberConverter,
        minValue: 0,
        maxValue: 8,
        ticks: [0, 1, 2, 3, 4, 5, 6, 7, 8],
        bins: [
            { displayName: '0', value: 0 },
            { displayName: '1', value: 1 },
            { displayName: '2', value: 2 },
            { displayName: '3', value: 3 },
            { displayName: '4', value: 4 },
            { displayName: '5', value: 5 },
            { displayName: '6', value: 6 },
            { displayName: '7', value: 7 },
            { displayName: '8', value: 8 }
        ],
        gradient: [
            {
                stop: 0,
                color: Colors.Blue
            },
            {
                stop: 1,
                color: Colors.Yellow
            }
        ]
    },
    {
        dbColumn: 'Parch',
        name: '# of Parents / Children aboard',
        type: DataRepresentation.Metric,
        isTimeBased: false,
        converter: numberConverter,
        minValue: 0,
        maxValue: 9,
        ticks: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9],
        bins: [
            { displayName: '0', value: 0 },
            { displayName: '1', value: 1 },
            { displayName: '2', value: 2 },
            { displayName: '3', value: 3 },
            { displayName: '4', value: 4 },
            { displayName: '5', value: 5 },
            { displayName: '6', value: 6 },
            { displayName: '7', value: 7 },
            { displayName: '8', value: 8 },
            { displayName: '9', value: 9 }
        ],
        gradient: [
            {
                stop: 0,
                color: Colors.Blue
            },
            {
                stop: 1,
                color: Colors.Yellow
            }
        ]
    },
    {
        dbColumn: 'Fare',
        name: 'Passenger Fare',
        type: DataRepresentation.Metric,
        isTimeBased: false,
        converter: numberConverter,
        minValue: 0,
        maxValue: 550,
        ticks: [0, 50, 100, 200, 300, 400, 500],
        bins: [
            { displayName: '<50', range: [0, 50] },
            { displayName: '50 - 100', range: [50, 100] },
            { displayName: '100 - 150', range: [100, 100] },
            { displayName: '150 - 200', range: [150, 200] },
            { displayName: '200 - 250', range: [200, 250] },
            { displayName: '250 - 300', range: [250, 300] },
            { displayName: '300 - 350', range: [200, 350] },
            { displayName: '350 - 400', range: [350, 400] },
            { displayName: '400 - 450', range: [400, 450] },
            { displayName: '450 - 500', range: [450, 500] },
            { displayName: '>500', range: [500, 550] },
        ],
        gradient: [
            {
                stop: 0,
                color: Colors.Green
            },
            {
                stop: 1,
                color: Colors.Red
            }
        ]
    },
    {
        dbColumn: 'Embarked',
        name: 'Port of Embarkation',
        type: DataRepresentation.Categorical,
        converter: (d: any) => {
            switch (d) {
                case 'Q':
                return 0;

                case 'C':
                return 1;

                case 'S':
                return 2;

                default:
                return 3;
            }
        },
        values: [
            {
                dbValue: 0,
                name: 'Queenstown',
                color: Colors.Red
            },
            {
                dbValue: 1,
                name: 'Cherbourg',
                color: Colors.Green
            },
            {
                dbValue: 2,
                name: 'Southampton',
                color: Colors.Blue
            },
            {
                dbValue: 3,
                name: 'Unknown',
                color: Colors.Yellow,
            },
        ]
    }
];
