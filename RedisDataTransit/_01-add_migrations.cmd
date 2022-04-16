for /f "delims=" %%# in ('powershell get-date -format "{yyyy_MM_dd_HH_mm_ss}"') do set vardatetime=%%#

dotnet ef migrations --startup-project ../RedisDataTransit/ add POST_%vardatetime% --context MyDbContext --verbose 
pause 