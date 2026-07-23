import { readFileSync } from "node:fs";
import { describe, expect, it } from "vitest";

const template = readFileSync("src/app/usermanagement/usermanagement-user.component.html", "utf8");

describe("UserManagementUserComponent template", () => {
    it("hides empty selectable quality profile controls", () => {
        expect(template).toContain('*ngIf="selectableRadarrQualities?.length > 0"');
        expect(template).toContain('*ngIf="selectableRadarr4KQualities?.length > 0"');
        expect(template).toContain('*ngIf="selectableSonarrQualities?.length > 0"');
    });
});
