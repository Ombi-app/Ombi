import { Component, computed, inject, signal, ChangeDetectionStrategy } from "@angular/core";

import { AuthService } from "../../../auth/auth.service";
import { DiscoverType } from "../carousel-list/carousel-list.component";

@Component({
    standalone: false,
    templateUrl: "./discover.component.html",
    styleUrls: ["./discover.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class DiscoverComponent {
    // Services using inject() function
    private authService = inject(AuthService);

    // Public constants
    public DiscoverType = DiscoverType;
    
    // State using signals
    public isAdmin = signal<boolean>(false);
    public seasonalMovieCount = signal<number>(0);
    
    // Computed properties
    public showSeasonal = computed(() => this.seasonalMovieCount() > 0);

    constructor() {
        // Initialize admin status
        this.isAdmin.set(this.authService.isAdmin());
    }

    public setSeasonalMovieCount(count: number): void {
        this.seasonalMovieCount.set(count);
    }
}
