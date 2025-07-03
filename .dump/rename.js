const fs = require('fs');
const args = require('minimist')(process.argv.slice(2));
const path = require("path");

const dirname = args.d ?? "";
const pattern = args.p ?? "";
const replacement = args.r ?? "";

const dir = path.join("./", dirname)
const files = fs.readdirSync(dir);

files.forEach( file => {
    fs.renameSync(path.join(dir, file), path.join(dir, file.replace(pattern, replacement)));
});

console.log(fs.readdirSync(dir));