REM $(SolutionDir)..\unity\Assets\Plugins\x86_64\
REM copy "$(SolutionDir)..\unity\Assets\Plugins\x86_64\*" "$(TargetDir)"
setlocal EnableDelayedExpansion

REM input
set solution_dir=%1
set project_dir=%2
set output_dir=%3

REM copy to unity
set unity_dir="%solution_dir%..\unity\Assets\Plugins\generated"
copy %output_dir%*.dll %unity_dir%
copy %project_dir%thirdparty\dll\* %unity_dir%

REM copy to GUI
set imp_test_dir="%solution_dir%build\bin\gui\"
copy "%output_dir%*.dll" "%imp_test_dir%"
copy "%project_dir%thirdparty\dll\*" "%imp_test_dir%"


