SRC=pv
TEST=ModelChain
BUILD=bin

win32:
	IF EXIST $(BUILD) ( \
		del /Q $(BUILD)\* \
	) ELSE ( \
		md $(BUILD) \
	)
	csc -lib:$(BUILD) -out:$(BUILD)\$(SRC).dll \
		-optimize -platform:x64 -target:library $(SRC)\$(SRC).cs

	csc -lib:$(BUILD) -out:$(BUILD)\$(TEST).exe -reference:$(SRC).dll \
		-optimize -platform:x64 -target:exe $(TEST)\$(TEST).cs 
	$(BUILD)\$(TEST).exe

