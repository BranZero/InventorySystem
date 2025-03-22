using ServerHead.Scripts;

namespace TestCase;

public class HashTest
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("HonnyIsHome1")]
    [TestCase("1234567890")]
    [TestCase("1")]
    public void PasswordTest(string s)
    {
        Assert.That(s, Is.Not.Null);
        string t = EncryptData.HashSha384(s);
        if(t == null)
        {
            Assert.Fail("t is null");
        }
        else
        {
            Assert.That(t.Length, Is.EqualTo(48));
        }
    }
}