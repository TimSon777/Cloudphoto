# Задание
https://docs.itiscl.ru/vvot/2023-2024/tasks/task02/task02.html

# Как сбилдить в определенный runtime
dotnet publish --runtime `{runtime}` --configuration Release -p:PublishSingleFile=true --self-contained true -p:AssemblyName=cloudphoto

# Доступные runtime
win-x64
linux-x64

Если нужен другой ([список](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)) напишите мне
