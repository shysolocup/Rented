let n = "guh";
let a = "[guh]abc[/guh] asdfjlksdf [guh]sdfjlksdf[/guh]";

let pattern = /\[guh\](.*?)\[\/guh\]/g

let match;
let matches = [];
while ((match = pattern.exec(a)) !== null) {
    matches.push(match[1]);
}

console.log(matches);