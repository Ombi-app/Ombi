import { Directive, Input, OnInit, TemplateRef, ViewContainerRef } from "@angular/core";
import { AuthService } from "../../auth/auth.service";

@Directive({
	selector: '[permission]',
})
export class RoleDirective implements OnInit {
	private roleName: string;

	private isHidden = true;

	@Input() public set permission(val: string) {
		if (val) {
			this.roleName = val;
			this.updateView();
		}
	}

	public constructor(private templateRef: TemplateRef<unknown>, private viewContainer: ViewContainerRef, private auth: AuthService) {}

	public ngOnInit(): void {
		this.updateView();
	}

	private updateView(): void {
		if (this.auth.hasRole(this.roleName) || this.auth.hasRole("admin")) {
			if (this.isHidden) {
				this.viewContainer.createEmbeddedView(this.templateRef);
				this.isHidden = false;
			}
		} else {
			this.viewContainer.clear();
			this.isHidden = true;
		}
	}
}
