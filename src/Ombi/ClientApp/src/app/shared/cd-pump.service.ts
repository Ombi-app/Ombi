import { ChangeDetectorRef, Injectable } from "@angular/core";

/**
 * Under Angular 22 this application ends up with the automatic change-detection
 * machinery bound to a different ApplicationRef than the one the routed components
 * live in, so zone/scheduler ticks never refresh those views. `ChangeDetectorRef`
 * still works when called directly, so this service keeps track of the currently
 * active routed components' change detectors and runs change detection on them on
 * demand.
 */
@Injectable({ providedIn: "root" })
export class CdPumpService {
    private readonly refs = new Set<ChangeDetectorRef>();
    private pumping = false;

    public register(ref: ChangeDetectorRef): void {
        this.refs.add(ref);
    }

    public unregister(ref: ChangeDetectorRef): void {
        this.refs.delete(ref);
    }

    public pump(): void {
        if (this.pumping) {
            return;
        }
        this.pumping = true;
        try {
            this.refs.forEach(ref => {
                try {
                    ref.detectChanges();
                } catch {
                    /* view destroyed or detection already running */
                }
            });
        } finally {
            this.pumping = false;
        }
    }
}
