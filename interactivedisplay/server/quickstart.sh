#!/bin/bash
echo "Switching to $1"

cp "sql.conf-$1.json" "sql.conf.json"
echo "Initialized configuration"

if [[ $2 ]]; then
    echo "Loading new saved state"
    cp "data/cache-$2.json" "data/cache.json"
else
    echo "Keeping current state"
fi

echo "Starting server"
node bin/boot.js
