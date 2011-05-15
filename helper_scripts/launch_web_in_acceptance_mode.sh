set RUNZ_ACCEPTANCE_MODE=ACCEPTANCE
LOCATION=`dirname "$0"`
echo $LOCATION
xsp4 --applications /:$LOCATION/../src/app/Frog.UI.Web --port 6502