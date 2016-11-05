#!/bin/bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

prepend() {
    while read line; do
        echo "$1 $line";
    done;
}

cd $DIR/server
npm install
npm start | prepend "[SERVER]" &

cd $DIR/client
npm install
npm start | prepend "[CLIENT]"
