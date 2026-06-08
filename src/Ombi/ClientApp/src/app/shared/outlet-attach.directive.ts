import { ChangeDetectorRef, ComponentRef, Directive } from "@angular/core";
import { RouterOutlet } from "@angular/router";
import { CdPumpService } from "./cd-pump.service";

/**
 * Applied to every `<router-outlet>`, this registers each activated route
 * component's change detector with the {@link CdPumpService} so that change
 * detection can be driven for routed views (see CdPumpService for why this is
 * needed under Angular 22).
 */
@Directive({
    selector: "router-outlet",
    standalone: true,
})
export class OutletAttachDirective {
    private ref: ChangeDetectorRef | null = null;

    constructor(private readonly outlet: RouterOutlet, private readonly pump: CdPumpService) {
        this.outlet.activateEvents.subscribe(() => {
            const componentRef = (this.outlet as unknown as { activated: ComponentRef<unknown> | null }).activated;
            // Use the activated component's *own* view ChangeDetectorRef (the one it
            // injects), not the host view's — only the former renders the component.
            this.ref = componentRef ? componentRef.injector.get(ChangeDetectorRef) : null;
            if (this.ref) {
                this.pump.register(this.ref);
                this.pump.pump();
            }
        });

        this.outlet.deactivateEvents.subscribe(() => {
            if (this.ref) {
                this.pump.unregister(this.ref);
                this.ref = null;
            }
        });
    }
}
