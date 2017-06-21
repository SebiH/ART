#!/bin/bash
set -eu

echo "Switching to $1"

TIMESTAMP=$(date +"%Y-%m-%d_%H-%M-%S")

cp "sql.conf.json" "sql.conf-$TIMESTAMP.json"
cp "sql.conf-$1.json" "sql.conf.json"
echo "Initialized configuration"

if [[ $2 ]]; then
    echo "Loading new saved state $2"
    cp "data/cache.json" "data/$TIMESTAMP-cache.json"
    cp "data/cache-$2.json" "data/cache.json"
else
    echo "Keeping current state"
fi

# if [[ $3 ]]; then
#     echo "Switching to database $3"
#     sed -i "s/\\\"database\\\": \\\"[^\\\"]*\\\"/\\\"database\\\": \\\"$3\\\"/g" sql.conf.json
# else
#     echo "Keeping current database"
# fi

echo "Starting server"
node bin/boot.js
