DBSTATUS=1
ERRCODE=1
i=0

while [[ $DBSTATUS -ne 0 ]] && [[ $i -lt 60 ]] && [[ $ERRCODE -ne 0 ]]; do
    i=$((i + 1))
    DBSTATUS=$(/opt/mssql-tools/bin/sqlcmd -h -1 -t 1 -U sa -P "$MSSQL_SA_PASSWORD" -Q "SET NOCOUNT ON; Select SUM(state) from sys.databases")
    ERRCODE=$?
    sleep 1
done
    
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$MSSQL_SA_PASSWORD" -d master -i setup.sql
