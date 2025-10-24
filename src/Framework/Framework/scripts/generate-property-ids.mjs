#!/usr/bin/env node

/**
 * Generates the DotvvmPropertyIdAssignment files:
 * - GroupMembers.cs - for property group member names
 * - TypeIds.cs - for control type IDs
 * - PropertyIds.cs - for individual property IDs
 * - PropertyGroupIds.cs - for property group IDs
 */

import fs from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const predefinedNames = [
    // HTML attributes
    'accept',
    'accesskey',
    'action',
    'align',
    'allow',
    'alt',
    'aria-checked',
    'aria-controls',
    'aria-describedby',
    'aria-expanded',
    'aria-hidden',
    'aria-label',
    'aria-selected',
    'as',
    'async',
    'autocomplete',
    'autofocus',
    'border',
    'charset',
    'checked',
    'class',
    'cols',
    'colspan',
    'content',
    'contenteditable',
    'crossorigin',
    'data-bind',
    'data-dismiss',
    'data-dotvvm-id',
    'data-placement',
    'data-target',
    'data-toggle',
    'data-ui',
    'data-uitest-name',
    'dir',
    'disabled',
    'download',
    'draggable',
    'enctype',
    'for',
    'form',
    'formaction',
    'formmethod',
    'formnovalidate',
    'formtarget',
    'height',
    'hidden',
    'href',
    'hreflang',
    'http-equiv',
    'id',
    'integrity',
    'itemprop',
    'lang',
    'list',
    'loading',
    'max',
    'maxlength',
    'media',
    'method',
    'min',
    'minlength',
    'multiple',
    'name',
    'novalidate',
    'pattern',
    'ping',
    'placeholder',
    'preload',
    'readonly',
    'referrerpolicy',
    'rel',
    'required',
    'role',
    'rows',
    'sandbox',
    'scope',
    'selected',
    'size',
    'slot',
    'span',
    'spellcheck',
    'src',
    'step',
    'style',
    'tabindex',
    'target',
    'title',
    'translate',
    'type',
    'value',
    'width',
    'wrap',

    // Common CSS properties 
    'background-color',
    'bottom',
    'color',
    'display',
    'font-size',
    'left',
    'line-height',
    'margin-bottom',
    'margin-right',
    'margin-top',
    'margin',
    'max-height',
    'max-width',
    'min-height',
    'min-width',
    'opacity',
    'padding-bottom',
    'padding-left',
    'padding-right',
    'padding-top',
    'padding',
    'position',
    'right',
    'top',
    'visibility',
    'z-index',

    // Common route parameter names
    'Id',
    'Name',
    'GroupId',
    'FileName',
    'UserId',
    'Slug',
    'slug',
    'Lang'
]

// Control types with their properties
// The script will automatically assign sequential IDs
const controls = [
    {
        name: 'DotvvmBindableObject',
        id: 1,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: ['DataContext'] // inherited
    },
    {
        name: 'DotvvmControl',
        id: 2,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['ID', 'ClientID', 'IncludeInPage'],
        slowProps: ['ClientIDMode'] // inherited
    },
    {
        name: 'HtmlGenericControl',
        id: 3,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Visible', 'InnerText'],
        slowProps: ['HtmlCapability'] // capability property
    },
    {
        name: 'RawLiteral',
        id: 4,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: []
    },
    {
        name: 'Literal',
        id: 5,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Text', 'FormatString', 'RenderSpanElement'],
        slowProps: []
    },
    {
        name: 'ButtonBase',
        id: 6,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Click', 'ClickArguments', 'Text'],
        slowProps: ['Enabled', 'TextOrContentCapability'] // with fallback, capability
    },
    {
        name: 'Button',
        id: 7,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['ButtonTagName', 'IsSubmitButton'],
        slowProps: []
    },
    {
        name: 'LinkButton',
        id: 8,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: []
    },
    {
        name: 'TextBox',
        id: 9,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Text', 'Changed', 'Type', 'TextInput', 'FormatString', 'SelectAllOnFocus'],
        slowProps: ['Enabled', 'UpdateTextOnInput'] // inherited
    },
    {
        name: 'RouteLink',
        id: 10,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['RouteName', 'Enabled', 'Text', 'UrlSuffix', 'Culture'],
        slowProps: []
    },
    {
        name: 'CheckableControlBase',
        id: 11,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Text', 'CheckedValue', 'Changed', 'LabelCssClass', 'InputCssClass', 'ItemKeyBinding'],
        slowProps: []
    },
    {
        name: 'CheckBox',
        id: 12,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Checked', 'CheckedItems', 'DisableIndeterminate'],
        slowProps: []
    },
    {
        name: 'RadioButton',
        id: 13,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Checked', 'CheckedItem', 'GroupName'],
        slowProps: []
    },
    {
        name: 'Validator',
        id: 14,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: ['HideWhenValid', 'InvalidCssClass', 'SetToolTipText', 'ShowErrorMessageText'] // inherited
    },
    {
        name: 'Validation',
        id: 15,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: ['Enabled', 'Target'] // inherited
    },
    {
        name: 'ValidationSummary',
        id: 16,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['IncludeErrorsFromChildren', 'HideWhenValid', 'IncludeErrorsFromTarget'],
        slowProps: []
    },
    {
        name: 'ItemsControl',
        id: 17,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['DataSource'],
        slowProps: []
    },
    {
        name: 'Repeater',
        id: 18,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['EmptyDataTemplate', 'ItemTemplate', 'RenderWrapperTag', 'SeparatorTemplate', 'WrapperTagName', 'RenderAsNamedTemplate'],
        slowProps: []
    },
    {
        name: 'HierarchyRepeater',
        id: 19,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['ItemChildrenBinding', 'ItemTemplate', 'EmptyDataTemplate', 'RenderWrapperTag', 'WrapperTagName'],
        slowProps: []
    },
    {
        name: 'GridView',
        id: 20,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['FilterPlacement', 'EmptyDataTemplate', 'Columns', 'RowDecorators', 'HeaderRowDecorators', 'EditRowDecorators', 'SortChanged', 'ShowHeaderWhenNoData', 'InlineEditing', 'LoadData'],
        slowProps: []
    },
    {
        name: 'GridViewColumn',
        id: 21,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['HeaderText', 'HeaderTemplate', 'FilterTemplate', 'SortExpression', 'SortAscendingHeaderCssClass', 'SortDescendingHeaderCssClass', 'AllowSorting', 'CssClass', 'IsEditable', 'HeaderCssClass', 'Width', 'Visible', 'CellDecorators', 'EditCellDecorators', 'EditTemplate', 'HeaderCellDecorators'],
        slowProps: []
    },
    {
        name: 'GridViewTextColumn',
        id: 22,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['FormatString', 'ChangedBinding', 'ValueBinding', 'ValidatorPlacement'],
        slowProps: []
    },
    {
        name: 'GridViewCheckBoxColumn',
        id: 23,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['ValueBinding', 'ValidatorPlacement'],
        slowProps: []
    },
    {
        name: 'GridViewTemplateColumn',
        id: 24,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['ContentTemplate'],
        slowProps: []
    },
    {
        name: 'DataPager',
        id: 25,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['DataSet', 'FirstPageTemplate', 'LastPageTemplate', 'PreviousPageTemplate', 'NextPageTemplate', 'RenderLinkForCurrentPage', 'HideWhenOnlyOnePage', 'LoadData'],
        slowProps: []
    },
    {
        name: 'AppendableDataPager',
        id: 26,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['LoadTemplate', 'LoadingTemplate', 'EndTemplate', 'DataSet', 'LoadData'],
        slowProps: []
    },
    {
        name: 'SelectorBase',
        id: 27,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['ItemTextBinding', 'ItemValueBinding', 'SelectionChanged', 'ItemTitleBinding'],
        slowProps: []
    },
    {
        name: 'Selector',
        id: 28,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['SelectedValue'],
        slowProps: []
    },
    {
        name: 'MultiSelector',
        id: 29,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['SelectedValues'],
        slowProps: []
    },
    {
        name: 'ListBox',
        id: 30,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Size'],
        slowProps: []
    },
    {
        name: 'ComboBox',
        id: 31,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['EmptyItemText'],
        slowProps: []
    },
    {
        name: 'SelectorItem',
        id: 32,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Text', 'Value'],
        slowProps: []
    },
    {
        name: 'FileUpload',
        id: 33,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['UploadedFiles', 'Capture', 'MaxFileSize', 'UploadCompleted'],
        slowProps: []
    },
    {
        name: 'Timer',
        id: 34,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Command', 'Interval', 'Enabled'],
        slowProps: []
    },
    {
        name: 'UpdateProgress',
        id: 35,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Delay', 'IncludedQueues', 'ExcludedQueues'],
        slowProps: []
    },
    {
        name: 'Label',
        id: 36,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['For'],
        slowProps: []
    },
    {
        name: 'EmptyData',
        id: 37,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['WrapperTagName', 'RenderWrapperTag'],
        slowProps: []
    },
    {
        name: 'Content',
        id: 38,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['ContentPlaceHolderID'],
        slowProps: []
    },
    {
        name: 'TemplateHost',
        id: 39,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Template'],
        slowProps: []
    },
    {
        name: 'AddTemplateDecorator',
        id: 40,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['AfterTemplate', 'BeforeTemplate'],
        slowProps: []
    },
    {
        name: 'SpaContentPlaceHolder',
        id: 41,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['DefaultRouteName', 'PrefixRouteName', 'UseHistoryApi'],
        slowProps: []
    },
    {
        name: 'ModalDialog',
        id: 42,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Open', 'CloseOnBackdropClick', 'Close'],
        slowProps: []
    },
    {
        name: 'HtmlLiteral',
        id: 43,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Html'],
        slowProps: []
    },
    {
        name: 'RequiredResource',
        id: 44,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Name'],
        slowProps: []
    },
    {
        name: 'InlineScript',
        id: 45,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Dependencies', 'Script'],
        slowProps: []
    },
    {
        name: 'RoleView',
        id: 46,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Roles', 'IsMemberTemplate', 'IsNotMemberTemplate', 'HideForAnonymousUsers'],
        slowProps: []
    },
    {
        name: 'ClaimView',
        id: 47,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Claim', 'Values', 'HasClaimTemplate', 'HideForAnonymousUsers'],
        slowProps: []
    },
    {
        name: 'EnvironmentView',
        id: 48,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Environments', 'IsEnvironmentTemplate', 'IsNotEnvironmentTemplate'],
        slowProps: []
    },
    {
        name: 'JsComponent',
        id: 49,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Global', 'Name', 'WrapperTagName'],
        slowProps: []
    },
    {
        name: 'PostBackHandler',
        id: 50,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['EventName', 'Enabled'],
        slowProps: []
    },
    {
        name: 'SuppressPostBackHandler',
        id: 51,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Suppress'],
        slowProps: []
    },
    {
        name: 'ConcurrencyQueueSetting',
        id: 52,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['EventName', 'ConcurrencyQueue'],
        slowProps: []
    },
    {
        name: 'NamedCommand',
        id: 53,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Name', 'Command'],
        slowProps: []
    },
    {
        name: 'PostBack',
        id: 54,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['Update', 'Handlers', 'ConcurrencyQueueSettings'],
        slowProps: ['Concurrency', 'ConcurrencyQueue'] // inherited
    },
    {
        name: 'FormControls',
        id: 55,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: ['Enabled'] // inherited
    },
    {
        name: 'UITests',
        id: 56,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: ['GenerateStub'] // inherited
    },
    {
        name: 'Events',
        id: 57,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [], // Uses ActiveDotvvmProperty.RegisterCommandToAttribute
        slowProps: []
    },
    {
        name: 'Styles',
        id: 58,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [], // Uses CompileTimeOnlyDotvvmProperty
        slowProps: []
    },
    {
        name: 'Internal',
        id: 59,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: ['UniqueID', 'IsNamingContainer', 'IsControlBindingTarget', 'PathFragment', 'IsServerOnlyDataContext', 'MarkupLineNumber', 'ClientIDFragment', 'IsMasterPageCompositionFinished', 'CurrentIndexBinding', 'ReferencedViewModuleInfo', 'UsedPropertiesInfo'],
        slowProps: ['IsSpaPage', 'UseHistoryApiSpaNavigation', 'DataContextType', 'MarkupFileName', 'RequestContext'] // inherited
    },
    {
        name: 'RenderSettings',
        id: 60,
        namespace: 'DotVVM.Framework.Controls',
        fastProps: [],
        slowProps: ['Mode'] // inherited
    },
    {
        name: 'DotvvmView',
        id: 61,
        namespace: 'DotVVM.Framework.Controls.Infrastructure',
        fastProps: [],
        slowProps: ['Directives'] // inherited
    }
]

const propertyGroups = [
    { type: 'HtmlGenericControl', name: 'Attributes' },
    { type: 'HtmlGenericControl', name: 'CssClasses' },
    { type: 'HtmlGenericControl', name: 'CssStyles' },
    { type: 'RouteLink', name: 'Params' },
    { type: 'RouteLink', name: 'QueryParameters' },
    { type: 'JsComponent', name: 'Props' },
    { type: 'JsComponent', name: 'Templates' }
]

// ------------------------------------------------------------------------------------------------

const csharpKeywords = new Set([
    'abstract', 'as', 'base', 'bool', 'break', 'byte', 'case', 'catch', 'char', 'checked',
    'class', 'const', 'continue', 'decimal', 'default', 'delegate', 'do', 'double', 'else',
    'enum', 'event', 'explicit', 'extern', 'false', 'finally', 'fixed', 'float', 'for',
    'foreach', 'goto', 'if', 'implicit', 'in', 'int', 'interface', 'internal', 'is',
    'lock', 'long', 'namespace', 'new', 'null', 'object', 'operator', 'out', 'override',
    'params', 'private', 'protected', 'public', 'readonly', 'ref', 'return', 'sbyte',
    'sealed', 'short', 'sizeof', 'stackalloc', 'static', 'string', 'struct', 'switch',
    'this', 'throw', 'true', 'try', 'typeof', 'uint', 'ulong', 'unchecked', 'unsafe',
    'ushort', 'using', 'virtual', 'void', 'volatile', 'while'
])

function csIdentifier(name) {
    let identifier = name.replace(/-/g, '_')
    return csharpKeywords.has(identifier) ? '@' + identifier : identifier
}

function validateAndGeneratePropertyDefinitions() {
    const propertyDefinitions = []
    const errors = []
    
    for (const control of controls) {
        if (control.fastProps.length > 16) {
            errors.push(`Control ${control.name} has ${control.fastProps.length} fast properties, maximum is 16`)
        }
        if (control.slowProps.length > 16) {
            errors.push(`Control ${control.name} has ${control.slowProps.length} slow properties, maximum is 16`)
        }
        
        // properties with CanUseFastAccessors=true get even IDs (last bit zero)
        control.fastProps.forEach((propName, index) => {
            propertyDefinitions.push({
                typeName: control.name,
                propertyName: propName,
                sequentialId: (index + 1) * 2,
                canUseFastAccessors: true
            })
        })

        // properties with CanUseFastAccessors=false get odd IDs (last bit one)
        control.slowProps.forEach((propName, index) => {
            propertyDefinitions.push({
                typeName: control.name,
                propertyName: propName,
                sequentialId: (index * 2) + 1,
                canUseFastAccessors: false
            })
        })
    }
    
    if (errors.length > 0) {
        console.error('❌ Configuration validation failed:')
        errors.forEach(error => console.error(`   ${error}`))
        throw new Error('Configuration validation failed')
    }
    
    return propertyDefinitions
}

function generateControlTypes() {
    return controls.map(control => ({
        name: control.name,
        id: control.id,
        namespace: control.namespace
    }))
}

function generateGroupMembersClass() {
    const set = new Set()
    const names = predefinedNames.filter(n => !set.has(n) && set.add(n))

    const constants = names.map((name, index) => {
        const id = index + 1 // Start from 1
        return `        public const ushort ${csIdentifier(name)} = ${id};`
    }).join('\n')
    
    const listItems = names.map(name => {
        return `            ("${name}", ${csIdentifier(name)})`
    }).join(',\n')
    
    const switchCases = names.map(name => {
        const identifier = csIdentifier(name)
        return `                "${name}" => ${identifier}`
    }).join(',\n')
    
    return `// Generated by scripts/generate-property-ids.mjs
using System;
using System.Collections.Immutable;

namespace DotVVM.Framework.Binding;

static partial class DotvvmPropertyIdAssignment
{
    public static class GroupMembers
    {
${constants}

        public static readonly ImmutableArray<(string Name, ushort ID)> List = ImmutableArray.Create(
${listItems}
        );

        public static ushort TryGetId(ReadOnlySpan<char> attr) =>
            attr switch {
${switchCases},
                _ => 0,
            };
    }
}
`
}

function generateTypeIdsClass() {
    const controlTypes = generateControlTypes()
    
    const constants = controlTypes.map(type => {
        return `public const ushort ${type.name} = ${type.id};`
    }).join('\n        ')
    
    const listItems = controlTypes.map(type => {
        return `(typeof(${type.name}), ${type.name})`
    }).join(',\n            ')
    
    const usingStatements = [...new Set(controlTypes.map(t => t.namespace))].map(ns => 
        `using ${ns};`
    ).join('\n')
    
    return `// Generated by scripts/generate-property-ids.mjs
using System;
using System.Collections.Immutable;
${usingStatements}

namespace DotVVM.Framework.Binding;

static partial class DotvvmPropertyIdAssignment
{
    public static class TypeIds
    {
        ${constants}

        public static readonly ImmutableArray<(Type type, ushort id)> List = ImmutableArray.Create(
            ${listItems}
        );
    }
}
`
}

function generatePropertyIdsClass() {
    const propertyDefinitions = validateAndGeneratePropertyDefinitions()
    const controlTypes = generateControlTypes()
    
    const constants = propertyDefinitions.map(prop => {
        const typeInfo = controlTypes.find(t => t.name === prop.typeName)
        if (!typeInfo) {
            throw new Error(`Type ${prop.typeName} not found in control types`)
        }
        
        return `/// <seealso cref="${prop.typeName}.${prop.propertyName}Property" />
        public const uint ${prop.typeName}_${prop.propertyName} = TypeIds.${prop.typeName} << 16 | ${prop.sequentialId};`
    }).join('\n\n        ')
    
    return `// Generated by scripts/generate-property-ids.mjs
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;

namespace DotVVM.Framework.Binding;

static partial class DotvvmPropertyIdAssignment
{
    public static class PropertyIds
    {
        ${constants}
    }
}
`
}

function generatePropertyGroupIdsClass() {
    const constants = propertyGroups.map((group, index) => {
        const id = index + 1;
        const constantName = `${group.type}_${group.name}`;
        return `/// <summary><see cref="${group.type}.${group.name}" /></summary>
        public const ushort ${constantName} = ${id};`
    }).join('\n\n        ')
    
    return `// Generated by scripts/generate-property-ids.mjs
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Binding;

static partial class DotvvmPropertyIdAssignment
{
    public static class PropertyGroupIds
    {
        ${constants}
    }
}
`
}


const __filename = fileURLToPath(import.meta.url)
const __dirname = path.dirname(__filename)

function main() {
    const baseDir = path.join(__dirname, '..')

    const files = [
        { name: 'GroupMembers', generator: generateGroupMembersClass },
        { name: 'TypeIds', generator: generateTypeIdsClass },
        { name: 'PropertyIds', generator: generatePropertyIdsClass },
        { name: 'PropertyGroupIds', generator: generatePropertyGroupIdsClass }
    ]
    
    let hasErrors = false
    
    for (const file of files) {
        try {
            const filepath = path.join(baseDir, 'Binding', `DotvvmPropertyIdAssignment.${file.name}.cs`)
            const content = file.generator().replace('\r\n', '\n')
            fs.writeFileSync(filepath, content, 'utf8')
        } catch (error) {
            console.error(`❌ Error generating ${file.name}:`, error.message)
            hasErrors = true
        }
    }
    
    if (hasErrors) {
        process.exit(1)
    }
}

main()
