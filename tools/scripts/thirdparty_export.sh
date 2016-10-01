#!/bin/bash
ZIPFILE="thirdparty.zip"

function fail {
    rm $ZIPFILE
    exit 1
}

CURR_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd $CURR_DIR/../

zip -r $ZIPFILE thirdparty || fail
scp $ZIPFILE dl.sebi.sh:/srv/www/dl/ || fail
rm $ZIPFILE
