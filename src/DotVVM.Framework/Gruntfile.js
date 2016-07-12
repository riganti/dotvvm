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
    });

    grunt.loadNpmTasks('grunt-runtime-t4-template-task');
}