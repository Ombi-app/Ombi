import { Component, Input } from "@angular/core";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

@Component({
    selector: "ngbd-modal-content",
template: `
  <div class="modal-header">
  </div>
  <div class="modal-body">
    <p>Hello, {{name}}!</p>
  </div>
  <div class="modal-footer">
    <button type="button" class="btn btn-danger-outline" (click)="activeModal.close('Close click')">Close</button>
  </div>
`,
})
export class AddPlexUserComponent  {
   
    @Input() public name: string;

  constructor(public activeModal: NgbActiveModal) {
      console.log("called");
  }

}
