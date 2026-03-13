hostname=`hostname`
if [[ $hostname == 'workbench-Development' ]]; then
	echo "master = true Development"
    dotnet com.mymooo.workbench.dll --environment Development
elif [[ $hostname == 'workbench-Test' ]]; then
	echo "master = true Test"
    dotnet com.mymooo.workbench.dll --environment Test
elif [[ $hostname == 'workbench-Preview' ]]; then
	echo "master = true Preview"
    dotnet com.mymooo.workbench.dll --environment Preview
elif [[ $hostname == 'workbench-Bug' ]]; then
	echo "master = true Bug"
    dotnet com.mymooo.workbench.dll --environment Bug
elif [[ $hostname == 'workbench-Preview2' ]]; then
	echo "master = true Preview2"
    dotnet com.mymooo.workbench.dll --environment Preview2
elif [[ $hostname =~ '-0' ]]; then
	echo "master = true"
    dotnet com.mymooo.workbench.dll --environment Production
else
	echo "master = false"
    dotnet com.mymooo.workbench.dll --environment Production.Slave
fi
