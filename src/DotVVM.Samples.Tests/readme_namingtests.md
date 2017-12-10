There is a lot of samples and it is difficult to keep track which one is tested and which one is not.
Therefore, there is a convention for naming the tests, which helps the **DotVVM.Samples.Tests.CompletenessChecker** tool to determine which samples are not tested.

## Naming The Tests

We have four types of tests. In test names, we use a different prefix, than in sample route names:

<table>
    <tr>
        <th>Prefix in Test Names</th>
        <th>Prefix in Sample Route Names</th>
    </tr>
    <tr>
        <td>Complex_</td>
        <td>ComplexSamples_</td>
    </tr>
    <tr>
        <td>Feature_</td>
        <td>FeatureSamples_</td>
    </tr>
    <tr>
        <td>Control_</td>
        <td>ControlSamples_</td>
    </tr>
    <tr>
        <td>Error_</td>
        <td>Errors_</td>
    </tr>
</table>

### Simple Case: One Test = One Sample

In the simplest case, there will be one UI test for a sample. In this case, the names must match (with respect to the different prefix), for example:

```
Test Name:      Control_TextBox_Format
Sample Name:    ControlSamples_TextBox_Format
```

### More Tests per Sample

Sometimes, one sample is reused from more tests. The tests should have the same name as the sample (with respect to the different prefix) and they should be distinguished by suffix, for example:

```
Test 1 Name:    Control_TextBox_Format_ClientRendering
Test 2 Name:    Control_TextBox_Format_ServerRendering
Sample Name:    ControlSamples_TextBox_Format
```

### Special Cases

Sometimes, the test needs to work with multiple views. To identify, that the test uses another view than that one identified by the test name, it should be marked with the `SampleReference` attribute and specify all other route names the test is using.

```
[TestMethod]
[SampleReference(nameof(SampleRouteUrls.Complex_SPARedirect_PageA)]
[SampleReference(nameof(SampleRouteUrls.Complex_SPARedirect_PageB)]
public void Complex_SPARedirect_Redirecting() {
    ...
}
```


## Running the Check

The **DotVVM.Samples.Tests.CompletenessChecker** can be executed directly from the VS. It prints out all the routes which are not referenced by any test (neither by the test name, not by the `SampleReference` attribute).

It can be used in the CI process to stop the build when any not-tested sample is found - it uses the exit code 1.
If everything is correct, the exit code is 0 and nothing is printed out.
