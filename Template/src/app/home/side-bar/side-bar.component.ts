import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-side-bar',
  templateUrl: './side-bar.component.html',
  styleUrls: ['./side-bar.component.scss']
})
export class SideBarComponent implements OnInit {

  constructor() { }

  ngOnInit(): void {
  }

  ShowHideCollapse(a:string){
    var elt = document.getElementById(a) ;
    console.log(elt)
      if (elt != null){
        if (elt.classList.contains("show")){
          elt.classList.remove('show')
        } 
          else {
            elt.classList.add('show') 
          }
          elt.classList.toggle('show')
        }

  }

}
