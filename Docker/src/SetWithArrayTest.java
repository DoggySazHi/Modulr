import java.util.*;
import org.junit.jupiter.api.*;

import static java.time.Duration.ofSeconds;
import static org.junit.jupiter.api.Assertions.assertTimeoutPreemptively;
import static org.junit.jupiter.api.Assertions.assertEquals;

/**
 * The test class TearCalculatorTest.
 *
 * @author  (your name)
 * @version (a version number or a date)
 */
public class SetWithArrayTest extends SetWithArray
{
    @Test
    public void testSetWithArrayUnion00()
    {
        int[] a = {1, 2, 3, 4, 5};
        SetWithArray swa = new SetWithArray(a);
        int[] b = {4, 5, 6, 7};
        int[] temp = swa.union(b);
        TreeSet<Integer> ans = new TreeSet<Integer>();
        for (int i = 1; i <= 7; i++)
            ans.add(new Integer(i));
        assertEquals(true, ans.size() == temp.length);
        for(int i = 0; i < temp.length; i++)
            assertEquals(true, ans.contains(temp[i]));
    }

    @Test
    public void testSetWithArrayIntersection00()
    {
        int[] a = {1, 2, 3, 4, 5};
        SetWithArray swa = new SetWithArray(a);
        int[] b = {4, 5, 6, 7};
        int[] temp = swa.intersection(b);
        TreeSet<Integer> ans = new TreeSet<Integer>();
        for (int i = 4; i <= 5; i++)
            ans.add(new Integer(i));
        assertEquals(true, ans.size() == temp.length);
        for(int i = 0; i < temp.length; i++)
            assertEquals(true, ans.contains(temp[i]));
    }

    @Test
    public void testSetWithArrayDifference00()
    {
        int[] a = {1, 2, 3, 4, 5};
        SetWithArray swa = new SetWithArray(a);
        int[] b = {4, 5, 6, 7};
        int[] temp = swa.difference(b);
        TreeSet<Integer> ans = new TreeSet<Integer>();
        for (int i = 1; i <= 3; i++)
            ans.add(new Integer(i));
        assertEquals(true, ans.size() == temp.length);
        for(int i = 0; i < temp.length; i++)
            assertEquals(true, ans.contains(temp[i]));
    }

    @Test
    public void testSetWithArrayIsSubSetOf00()
    {
        int[] a = {1, 2, 3, 4, 5};
        SetWithArray swa = new SetWithArray(a);
        assertEquals(true, swa.isSubSetOf(a));
        int[] b = {4, 5, 6, 7};
        int[] c = {1, 2, 3, 4, 5, 6, 7, 9};
        assertEquals(false, swa.isSubSetOf(b));
        assertEquals(true, swa.isSubSetOf(c));
    }

    @Test
    public void testSetWithArrayIsEqualTo00()
    {
        int[] a = {1, 2, 3, 4, 5};
        SetWithArray swa = new SetWithArray(a);
        assertEquals(true, swa.isEqualTo(a));
        int[] b = {4, 5, 6, 7};
        int[] c = {4};
        assertEquals(false, swa.isEqualTo(b));
        assertEquals(false, swa.isEqualTo(c));
    }

    @Test
    public void testSetWithArraySymmetricDifference00()
    {
        int[] a = {1, 2, 3, 4, 5};
        SetWithArray swa = new SetWithArray(a);
        int[] b = {4, 5, 6, 7};
        int[] temp = swa.symmetricDifference(b);
        TreeSet<Integer> ans = new TreeSet<Integer>();
        ans.add(new Integer(1));
        ans.add(new Integer(2));
        ans.add(new Integer(3));
        ans.add(new Integer(6));
        ans.add(new Integer(7));
        assertEquals(true, ans.size() == temp.length);
        for(int i = 0; i < temp.length; i++)
            assertEquals(true, ans.contains(temp[i]));
    }

    @Test
    public void testSetWithArrayisPartition00()
    {
        int[] a = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19};
        SetWithArray swa = new SetWithArray(a);
        List<int []> ans = new ArrayList<int[]>();
        int[] b = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        ans.add(b);
        int[] c = {10, 11, 12, 13, 14, 15, 16, 17};
        ans.add(c);
        int[] d = {18, 19};
        ans.add(d);
        assertEquals(true, swa.isPartition(ans));
        int[] e = {0};
        ans.add(e);
        assertEquals(false, swa.isPartition(ans));
    }

    @Test
    public void testSetWithArrayisPartition01()
    {
        int[] a = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15,
                16, 17, 18, 19};
        SetWithArray swa = new SetWithArray(a);
        List<int []> ans = new ArrayList<int[]>();
        int[] b = {0, 1, 2, 3, 4, 5, 6, 7, 8, 9};
        ans.add(b);
        assertEquals(false, swa.isPartition(ans));
        int[] c = {10, 11, 12, 13, 14, 15, 16, 17};
        ans.add(c);
        int[] d = {18, 19};
        ans.add(d);
        assertEquals(true, swa.isPartition(ans));
        int[] e = {0};
        int[] f = {11, 12};
        ans.add(e);
        assertEquals(false, swa.isPartition(ans));
        ans.remove(e);
        assertEquals(true, swa.isPartition(ans));
        ans.add(f);
        assertEquals(false, swa.isPartition(ans));
        ans.remove(a);
    }

    @Test
    public void testTimeOut()
    {
        assertTimeoutPreemptively(ofSeconds(1), () ->
        {
            int i = 0;
            while(true)
            {
                i++;
            }
        });
    }
}
