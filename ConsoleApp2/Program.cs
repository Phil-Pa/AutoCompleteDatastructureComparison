using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using System.Diagnostics;
using System.Text;

namespace ConsoleApp2
{

    public class Trie
    {
        private readonly List<Trie> _subtries = new List<Trie>();
        private readonly char letter = '#';

        public int Count { get; private set; }

        public Trie(char letter = '#')
        {
            this.letter = letter;
        }

        public bool AddWord(string word)
        {
            if (!word.All(c => c is >= 'a' and <= 'z'))
                return false;

            Count++;

            AddWordInternal(word);

            return true;
        }

        private void AddWordInternal(string word)
        {
            var currentTrie = this;
            foreach (var letter in word)
            {
                Trie? nextTrie = currentTrie.GetSubTreeContainingLetter(letter);

                if (nextTrie is null)
                {
                    var newTrie = new Trie(letter);
                    currentTrie._subtries.Add(newTrie);
                    nextTrie = newTrie;

                }
                currentTrie = nextTrie;
            }
        }

        private Trie? GetSubTreeContainingLetter(char letter)
        {
            foreach (var subtrie in _subtries)
            {
                if (subtrie.letter == letter)
                {
                    return subtrie;
                }
            }

            return null;
        }

        public bool ContainsWord(string word)
        {
            if (string.IsNullOrEmpty(word))
                return false;

            var currentTrie = this;
            foreach (var letter in word)
            {
                Trie? nextTrie = currentTrie.GetSubTreeContainingLetter(letter);

                if (nextTrie is null)
                {
                    return false;

                }
                currentTrie = nextTrie;
            }

            return true;
        }

        public List<string> RecommendWords(string input)
        {
            var results = new List<string>();

            if (string.IsNullOrEmpty(input))
                return results;

            var currentTrie = this;
            foreach (var letter in input)
            {
                Trie? nextTrie = currentTrie.GetSubTreeContainingLetter(letter);

                if (nextTrie is null)
                {
                    break;
                }
                currentTrie = nextTrie;
            }

            foreach (var subtrie in currentTrie._subtries)
            {
                results.Add(input + subtrie.TraceToEnd());
            }

            return results;
        }

        private string TraceToEnd()
        {
            var sb = new StringBuilder();
            var currentTrie = this;

            sb.Append(currentTrie.letter);

            if (currentTrie._subtries.Count > 0)
            {
                sb.Append(currentTrie._subtries[0].TraceToEnd());
            }

            return sb.ToString();
        }

        private void TraceToEndFast(StringBuilder sb)
        {
            var currentTrie = this;

            sb.Append(currentTrie.letter);

            if (currentTrie._subtries.Count > 0)
            {
                currentTrie._subtries[0].TraceToEndFast(sb);
            }
        }


        public List<string> RecommendWordsFast(string input)
        {
            var results = new List<string>();

            if (string.IsNullOrEmpty(input))
                return results;

            var currentTrie = this;
            foreach (var letter in input)
            {
                Trie? nextTrie = currentTrie.GetSubTreeContainingLetter(letter);

                if (nextTrie is null)
                {
                    break;
                }
                currentTrie = nextTrie;
            }

            var sb = new StringBuilder();

            foreach (var subtrie in currentTrie._subtries)
            {
                sb.Append(input);
                subtrie.TraceToEndFast(sb);
                results.Add(sb.ToString());
                sb.Clear();
            }

            return results;
        }
    }

    [SimpleJob]
    [HtmlExporter]
    [RPlotExporter]
    public class Program
    {
        public const string WordsFileName = "C:\\Users\\phil-\\source\\repos\\ConsoleApp2\\ConsoleApp2\\words.txt";

        [ParamsSource(nameof(Source))]
        public string[] Words { get; set; }

        public static IEnumerable<string[]> Source()
        {
            yield return File.ReadAllLines(WordsFileName);
            yield return File.ReadAllLines(WordsFileName).SelectSubListRandomly(50);
            yield return File.ReadAllLines(WordsFileName).SelectSubListRandomly(25);
            yield return File.ReadAllLines(WordsFileName).SelectSubListRandomly(10);
        }

        public Lazy<string[]> SearchInputs { get; set; } = new Lazy<string[]>(() =>
        {
            var allowedChars = "abcdefghijklmnopqrstuvwxyz";
            var numInputs = 50;
            var inputs = new string[numInputs];

            var sb = new StringBuilder();
            var random = new Random();

            for (int i = 0; i < numInputs; i++)
            {
                var wordLength = random.Next(2, 4);
                for (int j = 0; j < wordLength; j++)
                {
                    sb.Append(allowedChars[random.Next(allowedChars.Length)]);
                }
                inputs[i] = sb.ToString();
                sb.Clear();
            }
            return inputs;
        });

        public Lazy<Trie> MyTrie { get; set; } = new Lazy<Trie>(() =>
        {
            var trie = new Trie();
            var words = File.ReadAllLines(WordsFileName);
            for (int i = 0; i < words.Length; i++)
            {
                trie.AddWord(words[i]);
            }
            return trie;
        });

        public Lazy<Dictionary<string, int>> MyDictionary { get; set; } = new Lazy<Dictionary<string, int>>(() =>
        {
            var dictionary = new Dictionary<string, int>();
            var words = File.ReadAllLines(WordsFileName);
            for (int i = 0; i < words.Length; i++)
            {
                dictionary[words[i]] = 1;
            }
            return dictionary;
        });

        [Benchmark]
        public List<string> BenchRecommendWithTrie()
        {
            var results = new List<string>();
            var inputs = SearchInputs.Value;
            var myTrie = MyTrie.Value;
            for (int i = 0; i < inputs.Length; i++)
            {
                var recommendedWords = myTrie.RecommendWords(inputs[i]);
                results.AddRange(recommendedWords);
            }
            return results;
        }

        [Benchmark]
        public List<string> BenchRecommendWithTrieFast()
        {
            var results = new List<string>();
            var inputs = SearchInputs.Value;
            var myTrie = MyTrie.Value;
            for (int i = 0; i < inputs.Length; i++)
            {
                var recommendedWords = myTrie.RecommendWordsFast(inputs[i]);
                results.AddRange(recommendedWords);
            }
            return results;
        }

        [Benchmark]
        public List<string> BenchRecommendWithDictionary()
        {
            var results = new List<string>();
            var keys = MyDictionary.Value.Keys;
            var inputs = SearchInputs.Value;
            for (int i = 0; i < inputs.Length; i++)
            {
                foreach (var key in keys)
                {
                    if (key.StartsWith(inputs[i]))
                    {
                        results.Add(key);
                    }
                }
            }
            return results;
        }

        [Benchmark]
        public bool BenchAddAndCheckContainsWordWithTrie()
        {
            var trie = new Trie();

            for (int i = 0; i < Words.Length; i++)
            {
                trie.AddWord(Words[i]);
            }
            for (int i = 0; i < Words.Length; i++)
            {
                if (!trie.ContainsWord(Words[i]))
                {
                    return false;
                }
            }

            return true;
        }

        [Benchmark]
        public bool BenchAddAndCheckContainsWordWithDictionary()
        {
            var map = new Dictionary<string, int>();

            for (int i = 0; i < Words.Length; i++)
            {
                map[Words[i]] = 1;
            }
            for (int i = 0; i < Words.Length; i++)
            {
                if (!map.ContainsKey(Words[i]))
                {
                    return false;
                }
            }

            return true;
        }

        [Benchmark]
        public bool BenchAddAndCheckContainsWordWithList()
        {
            var list = new List<string>(Words.Length);

            for (int i = 0; i < Words.Length; i++)
            {
                list.Add(Words[i]);
            }
            for (int i = 0; i < Words.Length; i++)
            {
                if (!list.Contains(Words[i]))
                {
                    return false;
                }
            }

            return true;
        }

        static void Main(string[] args)
        {
            var summary = BenchmarkRunner.Run<Program>();
        }
    }

    public static class Extensions
    {
        public static string[] SelectSubListRandomly(this string[] array, int percent = 50)
        {
            var newArrayLength = (int)(percent / 100.0 * array.Length);
            var result = new string[newArrayLength];
            var random = new Random();

            for (int i = 0; i < newArrayLength; i++)
            {
                string next;
                do
                {
                    next = array[random.Next(array.Length)];
                } while (result.Contains(next));

                result[i] = next;
            }

            return result;
        }
    }
}