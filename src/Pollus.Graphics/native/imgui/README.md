The cimgui projects needs to be built with emsdk toolchain to be used with the linker in the csproj file for web builds.

#### Building the cimgui.a file from source
1. Setup [emscripten](https://emscripten.org/docs/getting_started/downloads.html), make sure to install the version that the dotnet workloads are using.  
As of writing this it is `3.1.34`

2. Clone `cimgui` project with `git clone https://github.com/cimgui/cimgui.git`  
Make sure to init submodules with `git submodule update --init --recursive`

3. Run the `emmake make` with the following makefile in the parent directory of cimgui (or modify it for your use)
```
CXX = em++
OUTPUT = cimgui.o
IMGUI_DIR:=./cimgui/imgui
CIMGUI_DIR:=./cimgui

SOURCES = $(CIMGUI_DIR)/cimgui.cpp $(IMGUI_DIR)/imgui.cpp $(IMGUI_DIR)/imgui_draw.cpp $(IMGUI_DIR)/imgui_demo.cpp $(IMGUI_DIR)/imgui_widgets.cpp $(IMGUI_DIR)/imgui_tables.cpp

USE_WASM = -s WASM=0
SIDE_MODULE = -s SIDE_MODULE=1

all: $(SOURCES) $(OUTPUT)

export IMGUI_IMPLEMENTATION=1
export IMGUI_ENABLE_FREETYPE=1

$(OUTPUT): $(SOURCES) 
	$(CXX) $(SOURCES) -r -std=c++11 -o $(OUTPUT) -O2 $(USE_WASM) -I$(IMGUI_DIR) -I$(CIMGUI_DIR)

clean:
	rm -f $(OUTPUT)
```

4. Run `emar rcs cimgui.a cimgui.o` command to create library file from the output of previous command

5. `cimgui.a` has to appear first in the `EmccExtraLDFlags` build property in the csproj file