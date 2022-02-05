<?xml version="1.0"?>
<xsl:stylesheet version="2.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:xs="http://www.w3.org/2001/XMLSchema"
                xmlns:fn="http://www.w3.org/2005/xpath-functions"
                xmlns:ms="urn:schemas-microsoft-com:xslt"
                xmlns:dt="urn:schemas-microsoft-com:datatypes"
                xmlns:trxfn="urn:trxfn"
                xmlns:trx="http://microsoft.com/schemas/VisualStudio/TeamTest/2010"
                >

    <xsl:output method="text"/>
    <xsl:strip-space elements="*"/>

    <xsl:param name="reportTitle" select="/trx:TestRun/@name" />

<!--https://github.com/ikatyang/emoji-cheat-sheet/blob/master/README.md-->
<!--
    :radio_button:
    :x:

    :white_circle:
    :grey_question:
-->

    <xsl:template match="/">
        <xsl:variable name="startTime" select="/trx:TestRun/trx:Times/@start" as="xs:date" />
        <xsl:variable name="finishTime" select="/trx:TestRun/trx:Times/@finish" as="xs:date" />
# Test Results - <xsl:value-of select="$reportTitle" />

Expand the following summaries for more details:

&lt;details&gt;
    &lt;summary&gt; Duration: <xsl:value-of select="trxfn:DiffSeconds($startTime, $finishTime)" /> seconds
    &lt;/summary&gt;

| **Times** | |
|--|--|
| **Started:**  | `<xsl:value-of select="$startTime" />` |
| **Creation:** | `<xsl:value-of select="/trx:TestRun/trx:Times/@creation" />`
| **Queuing:**  | `<xsl:value-of select="/trx:TestRun/trx:Times/@queuing" />`
| **Finished:** | `<xsl:value-of select="$finishTime" />` |
| **Duration:** | `<xsl:value-of select="trxfn:DiffSeconds($startTime, $finishTime)" />` seconds |

&lt;/details&gt;

&lt;details&gt;
    &lt;summary&gt; Outcome: <xsl:value-of select="/trx:TestRun/trx:ResultSummary/@outcome"
        /> | Total Tests: <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@total"
        /> | Passed: <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@passed"
        /> | Failed: <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@failed" />
    &lt;/summary&gt;

| **Counters** | |
|--|--|
| **Total:**               | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@total" /> |
| **Executed:**            | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@executed" /> |
| **Passed:**              | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@passed" /> |
| **Failed:**              | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@failed" /> |
| **Error:**               | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@error" /> |
| **Timeout:**             | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@timeout" /> |
| **Aborted:**             | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@aborted" /> |
| **Inconclusive:**        | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@inconclusive" /> |
| **PassedButRunAborted:** | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@passedButRunAborted" /> |
| **NotRunnable:**         | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@notRunnable" /> |
| **NotExecuted:**         | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@notExecuted" /> |
| **Disconnected:**        | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@disconnected" /> |
| **Warning:**             | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@warning" /> |
| **Completed:**           | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@completed" /> |
| **InProgress:**          | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@inProgress" /> |
| **Pending:**             | <xsl:value-of select="/trx:TestRun/trx:ResultSummary/trx:Counters/@pending" /> |

&lt;/details&gt;

## Tests:

        <xsl:apply-templates select="/trx:TestRun/trx:TestDefinitions"/>
    </xsl:template>

    <xsl:template match="trx:UnitTest">
        <xsl:variable name="testId"
                      select="@id" />
        <xsl:variable name="testResult"
                      select="/trx:TestRun/trx:Results/trx:UnitTestResult[@testId=$testId]" />
        <xsl:variable name="testOutcomeIcon">
            <xsl:choose>
                <xsl:when test="$testResult/@outcome = 'Passed'">:heavy_check_mark:</xsl:when>
                <xsl:when test="$testResult/@outcome = 'Failed'">:x:</xsl:when>
                <xsl:when test="$testResult/@outcome = 'NotExecuted'">:radio_button:</xsl:when>
                <xsl:otherwise>:grey_question:</xsl:otherwise>
            </xsl:choose>
        </xsl:variable>
<xsl:if test="$testResult/@outcome != 'Passed'">
&lt;details&gt;
    &lt;summary&gt;
<xsl:value-of select="$testOutcomeIcon" />
<xsl:text> </xsl:text>
<xsl:value-of select="@name" />
    &lt;/summary&gt;

| | |
|-|-|
| **Name:**          | `<xsl:value-of select="@name" />`
| **Class Name:**    | `<xsl:value-of select="trx:TestMethod/@className" />`
| **Outcome:**       | `<xsl:value-of select="$testResult/@outcome" />` <xsl:value-of select="$testOutcomeIcon" />
| **Duration:**      | `<xsl:value-of select="$testResult/@duration" />`

<xsl:if test="$testResult/@outcome = 'Failed'">

Error Message

```text
<xsl:value-of select="$testResult/trx:Output/trx:ErrorInfo/trx:Message" />
```

Error Stack Trace:

```text
<xsl:value-of select="$testResult/trx:Output/trx:ErrorInfo/trx:StackTrace" />
```
</xsl:if>

-----
&lt;/details&gt;
</xsl:if>
    </xsl:template>

</xsl:stylesheet>
