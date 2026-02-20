import { Component, OnInit } from "@angular/core";
import { Router } from "@angular/router";
import { StorageService } from "../shared/storage/storage-service";

function getCookieValue(name: string): string | undefined {
    const match = document.cookie.match(new RegExp('(?:^|; )' + name.replace(/([.$?*|{}()\[\]\\/+^])/g, '\\$1') + '=([^;]*)'));
    return match ? decodeURIComponent(match[1]) : undefined;
}

@Component({
        standalone: false,
    templateUrl: "cookie.component.html",
})
export class CookieComponent implements OnInit {
    constructor(private readonly router: Router,
                private store: StorageService) { }

    public ngOnInit() {
        const jwtVal = getCookieValue("Auth");
        if (jwtVal) {
            this.store.save("id_token", jwtVal);
            this.router.navigate(["discover"]);
        } else {
            this.router.navigate(["login"]);
        }
    }
}
