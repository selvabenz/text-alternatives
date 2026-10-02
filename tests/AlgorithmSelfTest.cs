using System;
using TamilDivineName.Paratext95;

public static class AlgorithmSelfTest
{
    private static int _fails;

    private static void Equal(string name, string expected, string actual)
    {
        if (expected == actual)
            Console.WriteLine("PASS " + name + " => " + actual);
        else
        {
            Console.WriteLine("FAIL " + name + " expected=[" + expected + "] actual=[" + actual + "]");
            _fails++;
        }
    }

    public static int Main()
    {
        Equal("ACC + ப => ப்",
            "ப்",
            TamilSandhiEngine.SuggestRightJoin("ACC", "புகழுங்கள்", false));

        Equal("ACC + த => த்",
            "த்",
            TamilSandhiEngine.SuggestRightJoin("ACC", "துதியுங்கள்", false));

        Equal("ACC left before கர்த்தர் => க் when project allows",
            "க்",
            TamilSandhiEngine.SuggestLeftJoin("ACC", "கர்த்தர்", false, true));

        Equal("ACC left before யெகோவா => none",
            "",
            TamilSandhiEngine.SuggestLeftJoin("ACC", "யெகோவா", false, true));

        Equal("punctuation blocks right join",
            "",
            TamilSandhiEngine.SuggestRightJoin("ACC", "புகழுங்கள்", true));

        Equal("NOM does not create right join",
            "",
            TamilSandhiEngine.SuggestRightJoin("NOM", "புகழுங்கள்", false));

        Console.WriteLine(_fails == 0 ? "ALL TESTS PASS" : _fails + " TEST(S) FAILED");
        return _fails == 0 ? 0 : 1;
    }
}
