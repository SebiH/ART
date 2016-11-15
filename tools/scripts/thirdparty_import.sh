#!/bin/bash

ZIPFILE="thirdparty.zip"

function fail {
    rm $ZIPFILE
    exit 1
}

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd $DIR/../

curl -o $ZIPFILE https://dl.sebi.sh/$ZIPFILE
unzip $ZIPFILE
rm $ZIPFILE
