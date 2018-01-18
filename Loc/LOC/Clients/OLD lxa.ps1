
Function BuildNugets
{
  write-host "SKIPPING Building PASNGR DB" -foregroundcolor "yellow"
  #BuildNuget lxa.database.pasngr "PASNGR DB"

  BuildNuget lxa.translations "Exsluts"

  BuildNuget lxa.database.pasngr.phraseelements "Phrase Elephants"

  BuildNuget lxa.database.pasngr.templates.salesconfirmation "Sales Conf"
  BuildNuget lxa.database.pasngr.templates.pdc "PDC"
  BuildNuget lxa.database.pasngr.templates.emd "EMD"
  BuildNuget lxa.database.pasngr.templates.boarding.checkin "chicken"

  BuildNuget lxa.CustisWrite "Custis"
}

Function GetOctopusProject
{
  "LXA-Templates"
}

