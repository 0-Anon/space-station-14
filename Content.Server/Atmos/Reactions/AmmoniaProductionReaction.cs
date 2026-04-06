using Content.Server.Atmos.EntitySystems;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Reactions;
using JetBrains.Annotations;

namespace Content.Server.Atmos.Reactions;

/// <summary>
///     Produces ammonia from nitrogen and water vapor, with plasma as a catalyst.
///     Requires a heat sweet spot. Needs a little bit of help over room temperature to get it going, but too much and the ammonia will decompose.
///     Similar to how Frezon works but way easier. Not punishing / throttling reaction for temp stuff, it either is too cold and not working or too hot and the ammonia decomposes. Simple to troubleshoot.
///     Designed to be forgiving to set up / a relatively simple reaction to teach but one that is hard to actually weaponize or to cause problems in function.
/// </summary>
[UsedImplicitly]
public sealed partial class AmmoniaProductionReaction : IGasReactionEffect
{
    public ReactionResult React(GasMixture mixture, IGasMixtureHolder? holder, AtmosphereSystem atmosphereSystem, float heatScale)
    {
        var initialN2 = mixture.GetMoles(Gas.Nitrogen);
        var initialWaterVapor = mixture.GetMoles(Gas.WaterVapor);
        var initialPlasma = mixture.GetMoles(Gas.Plasma);

        var efficiency = mixture.Temperature / Atmospherics.FrezonProductionMaxEfficiencyTemperature;
        var loss = 1 - efficiency;

        // How much the catalyst (Plasma) will allow us to produce
        var catalystLimit = initialPlasma * Atmospherics.AmmoniaProductionPlasmaRatio;

        // Amount of nitrogen and water vapor that are reacting
        var NitrogenBurned = Math.Min(CatalystLimit, initialN2);
        var VaporBurned = NitrogenBurned * Atmospherics.AmmoniaProductionVaporRatio;

        var NitrogenConversion = NitrogenBurned / Atmospherics.AmmoniaProductionConversionRate;
        var VaporConversion = VaporBurned / Atmospherics.AmmoniaProductionConversionRate;
        var total = NitrogenConversion + VaporConversion;

        mixture.AdjustMoles(Gas.Nitrogen, -NitrogenConversion);
        mixture.AdjustMoles(Gas.WaterVapor, -VaporConversion);
        mixture.AdjustMoles(Gas.Ammonia, total / 2);
        mixture.AdjustMoles(Gas.Oxygen, total * 0.375);

        return ReactionResult.Reacting;
    }
}
