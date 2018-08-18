import { Component, Input, OnInit } from "@angular/core";
import { FormBuilder, FormGroup, Validators } from "@angular/forms";
import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";

import { NotificationService, PlexService } from "../services";

import { IPlexLibraries, IPlexServersAdd } from "../interfaces";

@Component({
  selector: "ngbd-modal-content",
  templateUrl: "./addplexuser.component.html",
})
export class AddPlexUserComponent implements OnInit {

  @Input() public name: string;

  public plexServers: IPlexServersAdd[];
  public plexLibs: IPlexLibraries;

  public libsSelected: number[] = [];

  public form: FormGroup;

  constructor(public activeModal: NgbActiveModal,
              private plexService: PlexService,
              private notificationService: NotificationService,
              private fb: FormBuilder) {
  }

  public ngOnInit(): void {
    this.form = this.fb.group({
      selectedServer: [null, Validators.required],
      allLibsSelected: [true],
      username:[null, Validators.required],
    });
    this.getServers();
  }

  public getServers() {
    this.plexService.getServersFromSettings().subscribe(x => {
      if (x.success) {
        this.plexServers = x.servers;
      }
    });
  }

  public getPlexLibs(machineId: string) {
    this.plexService.getLibrariesFromSettings(machineId).subscribe(x => {
      if (x.successful) {
        this.plexLibs = x.data;
      }
    });
  }

  public selected() {
    this.getPlexLibs(this.form.value.selectedServer);
  }

  public checkedLib(checked: boolean, value: number) {
    if(checked) {
      this.libsSelected.push(value);
    } else {
      this.libsSelected = this.libsSelected.filter(v => v !== value);
    }
  }

  public onSubmit(form: FormGroup) {
    debugger;
    if (form.invalid) {
        this.notificationService.error("Please check your entered values");
        return;
    }
    const libs = form.value.allLibsSelected ? this.plexLibs.mediaContainer.directory.map(x => +x.key) : this.libsSelected;

    this.plexService.addUserToServer({ username: form.value.username, machineIdentifier: form.value.selectedServer, libsSelected: libs }).subscribe(x => {
      if (x.success) {
        this.notificationService.success("User added to Plex");
      } else {
        this.notificationService.error(x.error);
      }
    });

  }
}
