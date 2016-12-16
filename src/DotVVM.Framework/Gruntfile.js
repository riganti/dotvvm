/// <binding />
module.exports = function (grunt) {
    grunt.initConfig({
        runtime_t4_template_task: {
            options: {},
            "Hosting/ErrorPages/ErrorPageTemplate.cs": [{
                path: "Hosting/ErrorPages/ErrorPageTemplate.tt",
                className: "ErrorPageTemplate: IErrorWriter",
                namespace: "DotVVM.Framework.Hosting.ErrorPages"
            }],
            "Controls/FileUploadPageTemplate.cs": [{
                path: "Controls/FileUploadPageTemplate.tt",
                className: "FileUploadPageTemplate",
                namespace: "DotVVM.Framework.Controls"
            }],
            "ResourceManagement/ClientGlobalize/JQueryGlobalizeRegisterTemplate.cs": [{
                path: "ResourceManagement/ClientGlobalize/JQueryGlobalizeRegisterTemplate.tt",
                className: "JQueryGlobalizeRegisterTemplate",
                namespace: "DotVVM.Framework.ResourceManagement.ClientGlobalize"
            }]
        },
        typescript: {
            base: {
                src: [
                    "Resources/Scripts/DotVVM.DomUtils.ts",
                    "Resources/Scripts/DotVVM.Events.ts",
                    "Resources/Scripts/DotVVM.FileUpload.ts",
                    "Resources/Scripts/DotVVM.Globalize.ts",
                    "Resources/Scripts/DotVVM.PostBackHandlers.ts",
                    "Resources/Scripts/DotVVM.Promise.ts",
                    "Resources/Scripts/DotVVM.Serialization.ts",
                    "Resources/Scripts/DotVVM.Validation.ts",
                    "Resources/Scripts/DotVVM.ts",
                    "Resources/Scripts/DotVVM.Evaluator.ts",
                ],
                dest: "Resources/Scripts/DotVVM.js",
                options: {
                    sourceMap: true,
                    declaration: true
                }
            }
        }
    });

    grunt.loadNpmTasks('grunt-runtime-t4-template-task');

    grunt.loadNpmTasks('grunt-typescript');
}