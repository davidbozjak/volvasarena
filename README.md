# Völvas Arena
Inspired by Völva who glances the future in the norse mythology, this project allows you to simply simulate the price of an asset in a stock market and compare different strategies of buying and selling to find the best performing one(s)

For most uses, the file to look at is the BuyAndSellStrategy. That's where the different buy and sell strategies are implemented. To compare your own strategy simply make another method, and it will be entered into the arena via reflection.

The default behavior is to compare ALL buy strategies (n) and ALL sell strategies (m), meaning that the marketplace will simulate n*m isolated bots in the marketplace. To keep the runtime of simulation reasonable, for most uses some strategies can be skipped. This is achieved by adding SkipWhenFormingExecutionList attribute to the method.

Happy simulating!
