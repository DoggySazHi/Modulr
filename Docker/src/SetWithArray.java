import java.util.*;

/*
 *   You  may assume all int[] represent a set.  That is, it does NOT contain duplicates
 */

public class SetWithArray {
    private final int[] x;

    public SetWithArray() {
        x = new int[0];
    }

    public SetWithArray(int[] num) {
        x = num;
    }

    /*
     * returns an Set (array) containing all elements in x OR y
     */
    public int[] union(int[] y) {
        // let's play the game of "no dynamically resized arrays"
        Integer[] preset = combine(x, y);

        for (int i = 0; i < preset.length; i++)
            for (int j = i + 1; j < preset.length; j++)
                if (preset[i] != null && preset[i].equals(preset[j]))
                    preset[j] = null;
        return convert(preset);
    }

    /*
     * returns an Set (array) containing all elements in x AND y
     *
     *      if the intersection is empty, return an arrray of length 0
     *
     */
    public int[] intersection(int[] y) {
        int[] smallerArr;
        int[] largerArr;
        if (x.length < y.length) {
            smallerArr = x;
            largerArr = y;
        } else {
            smallerArr = y;
            largerArr = x;
        }

        Integer[] preset = new Integer[smallerArr.length];


        for (int i = 0; i < smallerArr.length; i++)
            if (contains(largerArr, smallerArr[i]))
                preset[i] = smallerArr[i];
        return convert(preset);
    }

    /*
     * returns an Set (array) containing all elements in x that are not in y
     *
     *      if the intersection is empty, return an arrray of length 0
     */
    public int[] difference(int[] y) {
        Integer[] process = combine(x, new int[0]);
        for (int k : y)
            for (int j = 0; j < process.length; j++)
                if (process[j] != null && process[j] == k)
                    process[j] = null;
        return convert(process);
    }

    /*
     * returns true if all elements of x are contained in y
     */
    public boolean isSubSetOf(int[] y) {
        for (int i : x)
            if (!contains(y, i))
                return false;
        return true;
    }

    /*
     * returns true if all elements in y are contained in x
     *              and if all elements in x are contained in y
     */
    public boolean isEqualTo(int[] y) {
        return isSubSetOf(y) && new SetWithArray(y).isSubSetOf(x);
    }

    /*
     * returns the set of elements which are in one of the set
     *         that is:  (x - y) union (y - x)
     *
     *      if the intersection is empty, return an array of length 0
     */
    public int[] symmetricDifference(int[] y) {
        return new SetWithArray(difference(y)).union(new SetWithArray(y).difference(x));
    }

    /*
     * returns true if all the collection sets in s form a partition of x
     *         you may assume that x is a universal set.
     *
     *         You might have to look up the definition of a partition and
     */
    public boolean isPartition(List<int[]> s) {
        
        Integer[] process = combine(x, new int[0]);

        for (int[] a : s) {
            for (int b : a) {
                boolean success = false;
                for (int i = 0; i < process.length; i++) {
                    if (process[i] != null) {
                        if (process[i] == b) {
                            process[i] = null;
                            success = true;
                            break;
                        }
                    }
                }
                if(!success)
                    return false;
            }
        }

        return convert(process).length == 0;

    }

    private int[] toArray(List<Integer> in) {
        int[] out = new int[in.size()];
        for (int i = 0; i < out.length; i++)
            out[i] = in.get(i);
        return out;
    }

    private Integer[] combine(int[] x, int[] y) {
        Integer[] preset = new Integer[x.length + y.length];
        for (int i = 0; i < preset.length; i++)
            preset[i] = i < x.length ? x[i] : y[i - x.length];
        return preset;
    }

    private int[] convert(Integer[] in) {
        int counter = 0;
        for (Integer i : in)
            if (i != null)
                counter++;

        int[] out = new int[counter];
        for (Integer integer : in)
            if (integer != null) {
                out[out.length - counter] = integer;
                counter--;
            }
        Arrays.sort(out);
        return out;
    }

    private boolean contains(int[] array, int number) {
        for (int j : array)
            if (j == number)
                return true;
        return false;
    }
}
