using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleApp1
{
    class Program
    {       
        static void Main(string[] args)
        {
            string line;

            // Read the file and display it line by line.  
            System.IO.StreamReader file = new System.IO.StreamReader(@"C:\Users\stewa\Desktop\Class\Comp440\FDS_HW\ConsoleApp1\ConsoleApp1\Input.txt");
            //Reads first line - names of the attributes in the table
            line = file.ReadLine();
            string[] attributeNames = line.Split(' ');
            List<List<String>> table = new List<List<String>>();

            

            while ((line = file.ReadLine()) != null)
            {                
                string[] attributeValues = line.Split(' ');
                List<string> attributeValuesList = new List<string>();
                foreach (string value in attributeValues)
                {
                    attributeValuesList.Add(value);
                }
                table.Add(attributeValuesList);
            }


            TableObject tableObject = new TableObject(table, attributeNames);

            //tableObject.PrintAttributeNames();
            //tableObject.PrintTableValues();
            tableObject.ProcessFDS();

            
        }

        
    }

    public class TableObject
    {
        List<List<String>> table;
        string[] attributeNames;
        int totalFDs = 0;
        int trivialFDs = 0;
        int nonTrivialFDs = 0;

        List<string> fds = new List<string>();

        //each list of ints is a combination of the table attributes
        List<List<int>> allAttributeCombos = new List<List<int>>();

        public TableObject(List<List<string>> newTable, string[] newAttributeNames)
        {
            this.table = newTable;
            this.attributeNames = newAttributeNames;
        }

        public void ProcessFDS()
        {
            List<int> indexList = new List<int>();
            //Just create list of indexes to process for power set
            for (int i = 0; i < attributeNames.Length; i++)
            {
                indexList.Add(i);
                //GetPossibleAttributeCombos(i);
            }

            //Get possible combinations of attributes
            var results = GetCombinations(indexList)
                .Where(x => x.Length >= 2);
            foreach (var items in results)
            {
                List<int> newCombo = items.OfType<int>().ToList();
                allAttributeCombos.Add(newCombo);
            }                

            //PrintAllAttributeCombos();

            //For each attribute combo, check against every attribute combination on the right hand side
            foreach (List<int> leftSideCombo in allAttributeCombos)
            {
                //Check against all attribute combos on the right hand side
                CheckComboAgainstAllRHSCombos(leftSideCombo);
            }

            Console.Write("\nTotal Functional Dependencies: " + totalFDs);
            Console.Write("\nTotal Trivial Dependencies: " + trivialFDs);
            Console.Write("\nTotal Non Trivial Dependencies: " + nonTrivialFDs);


            File.AppendAllText(@"C:\Users\stewa\Desktop\Class\Comp440\FDS_HW\output.txt", "\nTotal Functional Dependencies: " + totalFDs + Environment.NewLine);
            File.AppendAllText(@"C:\Users\stewa\Desktop\Class\Comp440\FDS_HW\output.txt", "\nTotal Trivial Functional Dependencies: " + trivialFDs + Environment.NewLine);
            File.AppendAllText(@"C:\Users\stewa\Desktop\Class\Comp440\FDS_HW\output.txt", "\nTotal Non Trivial Functional Dependencies: " + nonTrivialFDs + Environment.NewLine);

            foreach (string s in fds)
            {
                File.AppendAllText(@"C:\Users\stewa\Desktop\Class\Comp440\FDS_HW\output.txt", s + Environment.NewLine);
            }
        }

        void CheckComboAgainstAllRHSCombos(List<int> leftSideCombo)
        {
            foreach (List<int> attributeCombo in allAttributeCombos)
            {
                CheckFunctionalDependency(leftSideCombo, attributeCombo);
            }
        }

        

        bool CheckFunctionalDependency(List<int> leftSideCombo, List<int> attributeCombo)
        {
            //For every row in the table
            for (int row = 0; row < table.Count; row++)
            {
                //For each tuple of values, concatenate together
                string leftHandSideValue = "";
                foreach (int comboIndex in leftSideCombo)
                {
                    leftHandSideValue = leftHandSideValue + table[row][comboIndex];
                }

                string rightHandSideValue = "";
                foreach (int comboIndex in attributeCombo)
                {
                    rightHandSideValue = rightHandSideValue + table[row][comboIndex];
                }

                //Check each row left hand side against every other row left hand side to see if equal
                for (int nextRow = row; nextRow < table.Count; nextRow++)
                {
                    string newRowLeftHandSideValue = "";
                    foreach (int comboIndex in leftSideCombo)
                    {
                        newRowLeftHandSideValue = newRowLeftHandSideValue + table[nextRow][comboIndex];
                    }

                    //If the two row left hand side values are equal, then need to check if their respective right hand sides are equal
                    if (leftHandSideValue == newRowLeftHandSideValue)
                    {
                        //Get the new row right hand side value
                        string newRowRightHandSideValue = "";
                        foreach (int comboIndex in attributeCombo)
                        {
                            newRowRightHandSideValue = newRowRightHandSideValue + table[nextRow][comboIndex];
                        }

                        //Check if the right hand side values are the same. If not, this is not a functional dependency
                        if (rightHandSideValue != newRowRightHandSideValue)
                        {
                            return false;
                        }
                    }
                }
            }


            string fdString = "";
            //Get number of trivial and non-trivial FDs
            if (CheckIfTrivial(leftSideCombo, attributeCombo))
            {
                trivialFDs++;
                Console.Write("Trivial: ");
                fdString = fdString + "Trivial: ";
            }
            else
            {
                nonTrivialFDs++;
                Console.Write("Non-Trivial: ");
                fdString = fdString + "Non-Trivial: ";
            }


            //If this is a functional dependency, let's print it out!
            fdString = fdString + PrintFunctionalDependency(leftSideCombo, attributeCombo);
            fdString = fdString + " #" + totalFDs;
            fds.Add(fdString);
            Console.Write("#" + totalFDs + "\n");

            totalFDs++;
            return true;
        }

        bool CheckIfTrivial(List<int> leftSideCombo, List<int> attributeCombo)
        {
            //Every attribute in the right hand side needs to be in the left hand side
            // for it to be trivial

            
            foreach (int attributeIndex in attributeCombo)
            {
                if (!leftSideCombo.Contains(attributeIndex))
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<T[]> GetCombinations<T>(List<T> source)
        {
            for (var i = 0; i < (1 << source.Count); i++)
                yield return source
                   .Where((t, j) => (i & (1 << j)) != 0)
                   .ToArray();
        }

        void GetPossibleAttributeCombos(int attributeIndex)
        {
            //Go through every combo of attributes starting from the current one
            for (int i = attributeIndex; i < attributeNames.Length; i++)
            {
                allAttributeCombos.Add(GetLeftHandSideCombination(attributeIndex, i));
            }
        }

        List<int> GetLeftHandSideCombination(int attributeIndex, int i)
        {
            List<int> leftHandSideCombo = new List<int>();
            //Get all combos of attributes for left hand side
            for (int j = attributeIndex; j <= i; j++)
            {
                leftHandSideCombo.Add(j);
            }
            return leftHandSideCombo;
        }

        #region Print Functions


        string PrintFunctionalDependency(List<int> leftSideCombo, List<int> attributeCombo)
        {
            string fdString = "";
            fdString = fdString + "( ";
            Console.Write("( ");
            foreach (int attributeIndex in leftSideCombo)
            {
                fdString = fdString + attributeNames[attributeIndex] + " ";
                Console.Write(attributeNames[attributeIndex] + " ");
            }

            fdString = fdString + ") -> ( ";
            Console.Write(") -> ( ");
            foreach (int attributeIndex in attributeCombo)
            {
                fdString = fdString + attributeNames[attributeIndex] + " ";
                Console.Write(attributeNames[attributeIndex] + " ");
            }
            fdString = fdString + ")";
            Console.Write(")");

            return fdString;
        }


        void PrintCombo(List<int> newLeftHandSideCombo)
        {
            foreach (int index in newLeftHandSideCombo)
            {
                Console.Write(index + " ");
            }
        }

        void PrintAllAttributeCombos()
        {
            foreach (List<int> combo in allAttributeCombos)
            {
                PrintCombo(combo);
                Console.Write("\n");
            }
        }

        public void PrintAttributeNames()
        {
            //Print all attribute names - test
            foreach (string name in attributeNames)
            {
                Console.Write(name + " ");
            }
            Console.Write("\n");
        }

        public void PrintTableValues()
        {
            //Print table - test
            foreach (List<string> attribute in table)
            {
                foreach (string value in attribute)
                {
                    Console.Write(value + " ");
                }
                Console.Write("\n");
            }
        }

        #endregion
    }
}
