/*
 * This file is part of veraPDF Validation, a module of the veraPDF project.
 * Copyright (c) 2015-2026, veraPDF Consortium <info@verapdf.org>
 * All rights reserved.
 *
 * veraPDF Validation is free software: you can redistribute it and/or modify
 * it under the terms of either:
 *
 * The GNU General public license GPLv3+.
 * You should have received a copy of the GNU General Public License
 * along with veraPDF Validation as the LICENSE.GPL file in the root of the source
 * tree.  If not, see http://www.gnu.org/licenses/ or
 * https://www.gnu.org/licenses/gpl-3.0.en.html.
 *
 * The Mozilla Public License MPLv2+.
 * You should have received a copy of the Mozilla Public License along with
 * veraPDF Validation as the LICENSE.MPL file in the root of the source tree.
 * If a copy of the MPL was not distributed with this file, you can obtain one at
 * http://mozilla.org/MPL/2.0/.
 */
package org.verapdf.gf.model.impl.pd.colors;

import org.verapdf.cos.COSName;
import org.verapdf.cos.COSObjType;
import org.verapdf.cos.COSObject;
import org.verapdf.gf.model.factory.colors.ColorSpaceFactory;
import org.verapdf.gf.model.factory.functions.FunctionFactory;
import org.verapdf.gf.model.impl.containers.StaticContainers;
import org.verapdf.gf.model.impl.cos.GFCosUnicodeName;
import org.verapdf.gf.model.impl.pd.functions.GFPDFunction;
import org.verapdf.model.baselayer.Object;
import org.verapdf.model.coslayer.CosUnicodeName;
import org.verapdf.model.pdlayer.PDColorSpace;
import org.verapdf.model.pdlayer.PDSeparation;
import org.verapdf.pd.function.PDFunction;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * Adapts a Separation color space for a single named spot color and its tint transform.
 *
 * <p>A Separation space represents one colorant and defines an alternate color space
 * plus a tint-transform function that approximates its output. The constructor registers
 * instances by colorant name; {@code getareTintAndAlternateConsistent} compares repeated
 * definitions so inconsistent uses of one named ink can be detected. The registry and
 * inconsistency set live in shared validation containers, scoped by the parser lifecycle.</p>
 *
 * <p><b>.NET porting notes:</b></p>
 * <ul>
 *   <li>Suggested type: {@code VeraPdf.Validation.Gf.Model.Impl.Pd.Colors.GFPDSeparation},
 *       implementing {@code VeraPdf.Model.PDLayer.IPDSeparation}.</li>
 *   <li>Represent links as read-only collections and use the color/function factories;
 *       map COS name objects to the model's Unicode-name adapter.</li>
 *   <li>Replace static per-document registries with injected context. The equivalence
 *       check compares underlying PDF objects, so implement structural/equivalent COS
 *       comparison rather than relying on wrapper reference equality.</li>
 * </ul>
 *
 * @author Maksim Bezrukov
 */
public class GFPDSeparation extends GFPDColorSpace implements PDSeparation {
    /** Model type name for a Separation color space. */
    public static final String SEPARATION_TYPE = "PDSeparation";

    /** Link name for the function converting tint to alternate-space values. */
    public static final String TINT_TRANSFORM = "tintTransform";
    /** Link name for the fallback color space used to render the spot color. */
    public static final String ALTERNATE = "alternate";
    /** Link name for the named colorant. */
    public static final String COLORANT_NAME = "colorantName";

    /** Registers this definition so repeated spot-color definitions can be compared. */
    public GFPDSeparation(org.verapdf.pd.colors.PDSeparation simplePDObject) {
        super(simplePDObject, SEPARATION_TYPE);
        String name = simplePDObject.getColorantName().getString();
        if (StaticContainers.getSeparations().containsKey(name)) {
            StaticContainers.getSeparations().get(name).add(this);
        } else {
            final List<GFPDSeparation> separationList = new ArrayList<>();
            separationList.add(this);
            StaticContainers.getSeparations().put(name, separationList);
        }
    }

    @Override
    /** Checks whether same-named separations use equivalent alternate spaces and transforms. */
    public Boolean getareTintAndAlternateConsistent() {
        String name = ((org.verapdf.pd.colors.PDSeparation) simplePDObject).getColorantName().getString();

        if (StaticContainers.getInconsistentSeparations().contains(name)) {
            return Boolean.FALSE;
        }

        if (StaticContainers.getSeparations().get(name).size() > 1) {
            for (GFPDSeparation gfPDSeparation : StaticContainers.getSeparations().get(name)) {
                if (gfPDSeparation.equals(this)) {
                    continue;
                }

                COSObject alternateSpaceToCompare =
                        ((org.verapdf.pd.colors.PDSeparation) gfPDSeparation.simplePDObject).getAlternate().getObject();
                COSObject tintTransformToCompare =
                        ((org.verapdf.pd.colors.PDSeparation) gfPDSeparation.simplePDObject).getCosTintTransform();

                COSObject alternateSpaceCurrent =
                        ((org.verapdf.pd.colors.PDSeparation) simplePDObject).getAlternate().getObject();
                COSObject tintTransformCurrent =
                        ((org.verapdf.pd.colors.PDSeparation) simplePDObject).getCosTintTransform();

                if (!alternateSpaceToCompare.isEquivalentTo(alternateSpaceCurrent) || !tintTransformToCompare.isEquivalentTo(tintTransformCurrent)) {
                    StaticContainers.getInconsistentSeparations().add(name);
                    return Boolean.FALSE;
                }
            }
        }

        return Boolean.TRUE;
    }

    @Override
    /** Dispatches the three links exposed by the Separation model contract. */
    public List<? extends Object> getLinkedObjects(String link) {
        switch (link) {
            case ALTERNATE:
                return this.getAlternate();
            case COLORANT_NAME:
                return this.getColorantName();
            case TINT_TRANSFORM:
                return this.getTintTransform();
            default:
                return super.getLinkedObjects(link);
        }
    }

    /** Returns the optional alternate color-space link used for rendering the spot ink. */
    public List<PDColorSpace> getAlternate() {
        org.verapdf.pd.colors.PDColorSpace space =
                ((org.verapdf.pd.colors.PDSeparation) this.simplePDObject).getAlternate();
        PDColorSpace currentSpace = ColorSpaceFactory.getColorSpace(space);
        if (currentSpace != null) {
            List<PDColorSpace> colorSpace = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
            colorSpace.add(currentSpace);
            return Collections.unmodifiableList(colorSpace);
        }
        return Collections.emptyList();
    }

	/** Wraps the colorant name as a Unicode-name model object when the COS value is a name. */
	private List<CosUnicodeName> getColorantName() {
        COSObject name = ((org.verapdf.pd.colors.PDSeparation) this.simplePDObject).getColorantName();
        if (name.getType() == COSObjType.COS_NAME) {
            List<CosUnicodeName> list = new ArrayList<>(MAX_NUMBER_OF_ELEMENTS);
            list.add(new GFCosUnicodeName((COSName) name.getDirectBase()));
            return Collections.unmodifiableList(list);
        }
        return Collections.emptyList();
    }

	/** Creates the specialized function adapter for this colorant's tint transform. */
	private List<GFPDFunction> getTintTransform() {
        PDFunction pdFunction = ((org.verapdf.pd.colors.PDSeparation) this.simplePDObject).getTintTransform();
        if (pdFunction == null) {
            return Collections.emptyList();
        }
        GFPDFunction gfpdFunction = FunctionFactory.createFunction(pdFunction);
        return Collections.singletonList(gfpdFunction);
    }

    @Override
    /** Returns the Unicode name of the colorant, or null for a malformed non-name value. */
    public String getname() {
        COSObject name = ((org.verapdf.pd.colors.PDSeparation) this.simplePDObject).getColorantName();
        if (name.getType() == COSObjType.COS_NAME) {
            return ((COSName)name.getDirectBase()).getUnicodeValue();
        }
        return null;
    }
}
