window.isTestResourceLoaded = true;

let proof = document.createElement("h1");
proof.innerText = "The \"testResource_locationFallback\" script has been loaded.";

document.getElementsByTagName("body")[0]
    .appendChild(proof);
