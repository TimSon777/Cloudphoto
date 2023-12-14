# Как сбилдить в определенный runtime
dotnet publish --runtime `{runtime}` --configuration Release -p:PublishSingleFile=true --self-contained true -p:AssemblyName=cloudphoto
