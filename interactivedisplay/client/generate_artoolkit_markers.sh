#!/bin/bash
# See http://www.artoolworks.com/support/applications/marker/
# for options etc

MARKER_DPI=72
MARKER_PX=212
MARKER_BORDER_SIZE="0.1"
MARKER_BORDER_IS_WHITE="false"

# using 4x4 BCH 13 9 3 => 512 bar codes
MARKER_DIMENSIONS=4
MARKER_ECC="bch_13_9_3"
MARKER_MAX_NUM=511 # -1

mkdir -p markers

for i in `seq 0 $MARKER_MAX_NUM`;
do
    URL="http://www.artoolworks.com/support/app/marker.php?genImage\
&gen_single_number=${i}&ecc_type=${MARKER_ECC}&marker_size=${MARKER_PX}\
&marker_image_resolution=${MARKER_DPI}&border_size=${MARKER_BORDER_SIZE}\
&border_is_white=${MARKER_BORDER_IS_WHITE}&barcode_dimensions=${MARKER_DIMENSIONS}"
    OUTPUT="$(printf %03d $i).png"
    curl "$URL" -o "markers/$OUTPUT"
done
