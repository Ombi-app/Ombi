import { Component, OnInit } from "@angular/core";

import { AuthService } from "../../../auth/auth.service";
import { DiscoverType } from "../carousel-list/carousel-list.component";

@Component({
    templateUrl: "./discover.component.html",
    styleUrls: ["./discover.component.scss"],
})
export class DiscoverComponent implements OnInit {


    public DiscoverType = DiscoverType;
    public isAdmin: boolean;
    public showSeasonal: boolean;

    constructor(private authService: AuthService) { }

    public ngOnInit(): void {
        this.isAdmin = this.authService.isAdmin();
    }

    public setSeasonalMovieCount(count: number) {
        if (count > 0) {
            this.showSeasonal = true;
        }
    }
}
