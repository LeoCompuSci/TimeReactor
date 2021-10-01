using System;

namespace SimpleReactionMachine
{
    class Tester
    {
        private static IController controller;
        private static IGui gui;
        private static string displayText;
        private static int randomNumber;
        private static int passed = 0;

        private class DummyGui : IGui
        {

            private IController controller;

            public void Connect(IController controller)
            {
                this.controller = controller;
            }

            public void Init()
            {
                displayText = "?reset?";
            }

            public void SetDisplay(string msg)
            {
                displayText = msg;
            }
        }

        private class RndGenerator : IRandom
        {
            public int GetRandom(int from, int to)
            {
                return randomNumber;
            }
        }

        private static void Reset(string ch, IController controller, string msg)
        {
            try
            {
                controller.Init();
                Message(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void GoStop(string ch, IController controller, string msg)
        {
            try
            {
                controller.GoStopPressed();
                Message(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void InsertCoin(string ch, IController controller, string msg)
        {
            try
            {
                controller.CoinInserted();
                Message(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void Ticks(string ch, IController controller, int n, string msg)
        {
            try
            {
                for (int t = 0; t < n; t++) controller.Tick();
                Message(ch, msg);
            }
            catch (Exception exception)
            {
                Console.WriteLine("test {0}: failed with exception {1})", ch, msg, exception.Message);
            }
        }

        private static void Message(string ch, string msg)
        {
            if (msg.ToLower() == displayText.ToLower())
            {
                Console.WriteLine("test {0}: passed successfully", ch);
                passed++;
            }
            else
                Console.WriteLine("test {0}: failed with message ( expected {1} | received {2})", ch, msg, displayText);
        }


        static void Main(string[] args)
        {
            // run simple test
            EnhancedTest();
            Console.WriteLine("\n=====================================\nSummary: {0} tests passed out of 40", passed);
            Console.ReadKey();
        }

        private static void EnhancedTest()
        {
            //Construct a ReactionController
            controller = new EnhancedReactionController();
            gui = new DummyGui();

            //Connect them to each other
            gui.Connect(controller);
            controller.Connect(gui, new RndGenerator());

            //Reset the components()
            gui.Init();

            //Test the EnhancedReactionController
            //THE COMPLETELY CIRCLE
            Reset("1", controller, "Insert coin");
            GoStop("2", controller, "Insert coin");
            Ticks("3", controller, 1, "Insert coin");

            InsertCoin("4", controller, "Press GO!");

            Ticks("5", controller, 1, "Press GO!");
            
            Ticks("6", controller, 1001, "Insert coin");
            InsertCoin("7", controller, "Press GO!");
            InsertCoin("8", controller, "Press GO!");

            randomNumber = 102;
            GoStop("9", controller, "Wait...");
            GoStop("10", controller, "Insert coin");
            controller.CoinInserted();
            controller.GoStopPressed();
            //WAIT tick(s)
            Ticks("11", controller, randomNumber - 1, "Wait...");

            //RUN tick(s)
            Ticks("12", controller, 11, "0.10");
            Ticks("13", controller, 111, "1.21");

            //goStop
            GoStop("14", controller, "1.21");
            InsertCoin("15", controller, "1.21");

            GoStop("16", controller, "Wait...");
            Ticks("17", controller, randomNumber - 1, "Wait...");
            Ticks("18", controller, 23, "0.22");
            Ticks("19", controller, 122, "1.44");
            GoStop("20", controller, "1.44");


            GoStop("21", controller, "Wait...");
            Ticks("22", controller, randomNumber - 1, "Wait...");
            Ticks("23", controller, 11, "0.10");
            Ticks("24", controller, 90, "1.00");
            GoStop("25", controller, "1.00");
            //STOP tick(s)
            Ticks("26", controller, 301, "Average: 1.22");
            Ticks("27", controller, 501, "Insert Coin");

            // IDLD >> READY >> TICK>10S >> IDLE???
            InsertCoin("28", controller, "Press GO!");
            Ticks("29", controller, 1001, "Insert Coin");


            //IDLE >> READY >> WAIT >pressSTOP> IDLE???
            controller.CoinInserted();
            GoStop("30", controller, "Wait...");
            GoStop("31", controller, "Insert coin");

            //IDLE >> READY >> WAIT >> RUN???
            gui.Init();
            Reset("32", controller, "Insert coin");
            controller.CoinInserted();
            controller.GoStopPressed();
            Ticks("33", controller, randomNumber - 1, "Wait...");

            Ticks("34", controller, 9, "0.08");
            Ticks("35", controller, 22, "0.30");

            //IDLE -> READY -> WAIT -> RUN (timeout) -> STOP
            gui.Init();
            Reset("36", controller, "Insert coin");
            randomNumber = 120;
            InsertCoin("37", controller, "Press GO!");
            GoStop("38", controller, "Wait...");
            Ticks("39", controller, randomNumber + 199, "1.99");
            Ticks("40", controller, 50, "2.00");
        }
    }

}
