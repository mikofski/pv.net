PROJ=pv
SRC=solarposition
TEST=ModelChain
BUILD=bin

win32:
	IF EXIST $(BUILD) ( \
		del /Q $(BUILD)\* \
	) ELSE ( \
		md $(BUILD) \
	)
	csc -lib:$(BUILD) -out:$(BUILD)\$(PROJ).dll \
		-optimize -platform:x64 -target:library $(PROJ)\$(SRC).cs

	csc -lib:$(BUILD) -out:$(BUILD)\$(TEST).exe -reference:$(PROJ).dll \
		-optimize -platform:x64 -target:exe $(TEST)\$(TEST).cs 
	$(BUILD)\$(TEST).exe

