RUNTIME=linux-x64
OUTPUT_DIR=bin/Release/$(RUNTIME)
OUTPUT_NAME=App

all: build

build: 
	dotnet publish App/App.csproj -r $(RUNTIME) -c Release -o $(OUTPUT_DIR) --self-contained true
	mv bin/Release/linux-x64/App bin/Release/ipk24chat-client
	mv bin/Release/ipk24chat-client ./ # Move to the root directory

clean:
	dotnet clean App
	rm ipk24chat-client
	rm -rf bin